namespace Quva.Services.Devices.Modbus;

public interface IModbusApi
{
    ModbusData Data { get; set; }
    Task<ModbusData> ModbusCommand(string command, string variableName, string value);
}

/// <summary>
/// Implements Adapter between DeviceParameter and Modbus Read/Write Functions in ModbusPort
/// </summary>
public class ModbusData : DeviceData
{
    //für Modbus:
    public IDictionary<string, ModbusBlock> modbusBlocks { get; set; }
    public IDictionary<string, ModbusVariable> modbusVariables { get; set; }
    //runtime:
    public IList<string> changedBlocks { get; set; }
    public string ReadBlockName = string.Empty;

    public ModbusData(string deviceCode, string command, DeviceOptions? deviceOptions) : base(deviceCode, command)
    {
        modbusBlocks = new Dictionary<string, ModbusBlock>();
        modbusVariables = new Dictionary<string, ModbusVariable>();
        changedBlocks = new List<string>();

        if (deviceOptions == null || deviceOptions.Options == null) return;
        // load Blocks and Variables from Device.Options
        foreach (var option in deviceOptions.Options)
        {
            var values = option.Value.Split('|');
            if (values[0] == ModbusFunction.ReadCoils.ToString() ||
                values[0] == ModbusFunction.ReadDiscreteInputs.ToString() ||
                values[0] == ModbusFunction.ReadHoldingRegisters.ToString() ||
                values[0] == ModbusFunction.ReadInputRegisters.ToString() ||
                values[0] == ModbusFunction.WriteSingleCoil.ToString() ||
                values[0] == ModbusFunction.WriteSingleRegister.ToString() ||
                values[0] == ModbusFunction.ToggleSingleRegister.ToString() ||
                values[0] == ModbusFunction.WriteMultipleCoils.ToString() ||
                values[0] == ModbusFunction.WriteMultipleRegisters.ToString())
            {
                modbusBlocks.Add(option.Key, new ModbusBlock(values));
                changedBlocks.Add(option.Key);
            }
        }
        // variables _after_ blocks
        foreach (var option in deviceOptions.Options)
        {
            var values = option.Value.Split('|');
            if (values[0] == ModbusDatatype.Bit.ToString() ||
                values[0] == ModbusDatatype.Word.ToString() ||
                values[0] == ModbusDatatype.Float.ToString())
            {
                if (modbusBlocks.TryGetValue(values[1], out var block))
                {
                    modbusVariables.Add(option.Key, new ModbusVariable(values, block));
                }
                else
                {
                    throw new ArgumentException($"wrong block ({values[1]})", nameof(deviceOptions));
                }
            }
        }
        // else option not for us, no error, E.g. comments
    }

    public override void Reset()
    {
        base.Reset();
    }

    public ByteBuff ReadBlockCommand(string blockName)
    {
        //building byte buffer for Read
        // buffer: <function:1>|<Adress:2>|<Count:1>
        var byteList = new List<Byte>();
        if (!modbusBlocks.TryGetValue(blockName, out ModbusBlock? block))
        {
            throw new ArgumentException($"unknown Block {blockName}", nameof(blockName));
        }


        byte[] byteAddress = BitConverter.GetBytes((short)block.address);
        byteList.Add((byte)block.function);
        byteList.Add(byteAddress[0]);
        byteList.Add(byteAddress[1]);
        byteList.Add((byte)block.quantity);

        ByteBuff result = new(byteList.ToArray(), byteList.Count);
        return result;
    }

    public ByteBuff WriteVariableCommand(string variableName, string value)
    {
        //building byte buffer for Write
        // buffer: <function:1>|<Adress:2>|<Count:1>[|<values as byte array>]
        var byteList = new List<Byte>();
        if (!modbusVariables.TryGetValue(variableName, out ModbusVariable? variable))
        {
            throw new ArgumentException($"unknown Variable {variableName}", nameof(variableName));
        }
        if (variable.block.isCoil && variable.datatype != ModbusDatatype.Bit)
        {
            throw new ArgumentException($"Bit Variable can only serv Coil Block ({variableName})", nameof(variableName));
        }
        ModbusBlock block = variable.block;
        int startAddress = block.address;
        int startBlock = 0;
        byte[] byteData = variable.datatype switch
        {
            ModbusDatatype.Bit => BitConverter.GetBytes(int.Parse(value) != 0),
            ModbusDatatype.Word => BitConverter.GetBytes((short)int.Parse(value)),
            ModbusDatatype.Float => BitConverter.GetBytes(float.Parse(value)),
            _ => throw new ArgumentException($"wrong datatype {variable.datatype}", nameof(variableName)),
        };
        if (variable.datatype == ModbusDatatype.Bit && !block.isCoil)
        {
            // toggle means the edge counts (0 to 1)
            int oldValue = block.isToggle ? 0 : BitConverter.ToInt16(block.data, 0);
            int newValue = oldValue | 1 << variable.offset;
            byteData = BitConverter.GetBytes((short)newValue);
        }
        else
        {
            startAddress += variable.offset;
            if (block.isCoil)
            {
                startBlock += variable.offset;
            }
            else
            {
                startBlock += 2 * variable.offset;  //16bit register
            }
        }
        byte[] byteAddress = BitConverter.GetBytes((short)startAddress);
        byteList.Add((byte)block.function);
        byteList.Add(byteAddress[0]);
        byteList.Add(byteAddress[1]);
        byteList.Add((byte)byteData.Length);
        foreach (byte b in byteData)
        {
            byteList.Add(b);
        }
        ByteBuff result = new(byteList.ToArray(), byteList.Count);
        // update block:
        for (int i = 0; i < byteData.Length; i++)
        {
            block.data[startBlock + i] = byteData[i];
        }
        return result;
    }

    public string GetValue(string variableName)
    {
        if (modbusVariables.TryGetValue(variableName, out var variable))
        {
            // calculate from block.data
            return variable.GetValue().ToString();
        }
        return string.Empty;
    }

    public void SetBlockData(string blockName, ByteBuff readData)
    {
        bool changed = false;
        if (modbusBlocks.TryGetValue(blockName, out var block))
        {
            for (int i = 0; i < readData.Cnt; i++)
            {
                if (block.data[i] != readData.Buff[i])
                {
                    changed = true;
                    block.data[i] = readData.Buff[i];
                }
            }
        }
        if (changed)
        {
            if (!changedBlocks.Contains(blockName))
                changedBlocks.Add(blockName);
        }
    }

}

public record ModbusVariable
{
    //from Data:
    public ModbusDatatype datatype;
    public string blockName;
    public int offset;  // Bit: Bit Position;  Word, Float: Register Units (16bit)
    public ModbusBlock block;

    public ModbusVariable(string[] values, ModbusBlock modbusBlock)
    {
        datatype = (ModbusDatatype)Enum.Parse(typeof(ModbusDatatype), values[0]);
        blockName = values[1];
        offset = values.Length > 2 ? int.Parse(values[2]) : 0;
        block = modbusBlock;
    }

    public float GetValue()
    {
        //extracts the typed value from block.data
        float result = 0;
        if (block.isCoil && datatype != ModbusDatatype.Bit)
        {
            throw new ArgumentException($"GetValue: Bit Variable can only serv Coil Block ({blockName})", nameof(blockName));
        }
        switch (datatype)
        {
            case ModbusDatatype.Bit:
                if (block.isCoil)
                {
                    result = block.data[0];
                }
                else
                {
                    int register = BitConverter.ToInt16(block.data, 0);
                    result = (register & (1 << offset)) != 0 ? 1 : 0;
                }
                break;
            case ModbusDatatype.Word:
                result = BitConverter.ToInt16(block.data, offset);
                break;
            case ModbusDatatype.Float:
                result = BitConverter.ToSingle(block.data, offset);
                break;
        }
        return result;
    }
}

public record ModbusBlock
{
    public ModbusFunction function;
    public int address;
    public int quantity;
    public bool isToggle;
    //runtime:
    public byte[] data;

    public bool isCoil
    {
        get => function == ModbusFunction.ReadCoils || function == ModbusFunction.ReadDiscreteInputs ||
            function == ModbusFunction.WriteMultipleCoils || function == ModbusFunction.WriteSingleCoil;
    }
    public bool isOutput
    {
        get => function == ModbusFunction.WriteMultipleRegisters || function == ModbusFunction.WriteMultipleCoils ||
            function == ModbusFunction.WriteSingleRegister || function == ModbusFunction.WriteSingleCoil;
    }
    public int byteCount
    {
        get => isCoil ? quantity : 2 * quantity;
    }

    public ModbusBlock(string[] values)
    {
        if (values[0] == ModbusFunction.ToggleSingleRegister.ToString())
        {
            isToggle = true;  //set bit=0 after sent
            function = ModbusFunction.WriteSingleRegister;
        }
        else
        {
            isToggle = false;
            function = (ModbusFunction)Enum.Parse(typeof(ModbusFunction), values[0]);
        }
        address = int.Parse(values[1]);
        quantity = values.Length > 2 ? int.Parse(values[2]) : 1;

        if (address > ushort.MaxValue)
        {
            throw new ArgumentException($"address too large ({address})");
        }
        if (quantity > 125)
        {
            throw new ArgumentException($"quantity too large ({quantity})");
        }

        data = new byte[byteCount];
    }
}

public enum ModbusDatatype
{
    None,
    Bit,
    Word,
    Float
}

public enum ModbusFunction
{
    ReadCoils = 1,
    ReadDiscreteInputs = 2,
    ReadHoldingRegisters = 3,
    ReadInputRegisters = 4,
    WriteSingleCoil = 5,
    WriteSingleRegister = 6,
    ToggleSingleRegister = 106,
    WriteMultipleCoils = 15,
    WriteMultipleRegisters = 16
}

// Commands for Modbus:
public enum ModbusCommands
{
    None,
    ReadBlocks,
    ReadBlock,
    WriteVariable
}


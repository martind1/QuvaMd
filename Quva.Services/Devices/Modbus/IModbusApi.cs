using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Quva.Services.Devices.Modbus.EasyModbus.ModbusServer;

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
    public ModbusData(string deviceCode, string command) : base(deviceCode, command)
    {
        modbusVariables = new Dictionary<string, ModbusVariable>();
    }

    public override void Reset()
    {
        base.Reset();
    }

    public string GetValue(string variableName)
    {
        if (modbusVariables.TryGetValue(variableName, out var variable))
        {
            return variable.value;
        }
        return string.Empty;
    }

    //für Modbus:
    public IDictionary<string, ModbusVariable> modbusVariables { get; set; }
}

public record ModbusBlock
{
    public string blockName = string.Empty;
    public ModbusFunction function;
    public int address;
    public int quantity;

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
}

public record ModbusVariable
{
    //from Data:
    public string name = string.Empty;
    public string functionName = string.Empty;
    public string typeName = string.Empty;
    public int address;
    public int bitOffset;
    //calculated:
    public ModbusFunction function;
    public ModbusDatatype datatype;
    public bool isCoil;
    public bool isOutput;
    //runtime:
    public string value = string.Empty;
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
    WriteMultipleCoils = 15,
    WriteMultipleRegisters = 16
}

// Commands for Modbus:
public enum ModbusCommands
{
    None,
    IOLoop,
    ReadBlocks,
    WriteVariable
}


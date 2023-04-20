namespace Quva.Services.Devices.Modbus;

public class WagoController : ComProtocol, IModbusApi
{
    public ModbusData Data { get; set; }
    private readonly DeviceOptions _deviceOptions;
    private bool _readAnswered = false;
    private readonly SemaphoreSlim _slim;

    public string[] WagoControllerDescription =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "T:500", //waiting for wago response
        "B:",
        "A:"
    };

    public WagoController(ComDevice device) : base(device.Code, device.ComPort)
    {
        Description = WagoControllerDescription;
        OnAnswer = WagoControllerAnswer;
        Data = new ModbusData(device.Code, ModbusCommands.ReadBlocks.ToString(), device.Options);

        ArgumentNullException.ThrowIfNull(device.Options);
        _deviceOptions = device.Options;
        _slim = new SemaphoreSlim(1);
    }

    public async Task<ModbusData> ModbusCommand(string command, string variableName, string value)
    {
        if (Enum.TryParse(command, out ModbusCommands cmd))
        {
            var data = cmd switch
            {
                ModbusCommands.ReadBlocks => await ReadBlocks(),
                ModbusCommands.ReadBlock => await ReadBlock(variableName),  //test
                ModbusCommands.WriteVariable => await WriteVariable(variableName, value),
                _ => throw new NotImplementedException($"ModbusCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }
        throw new NotImplementedException($"ModbusCommand({command}) not implemented");
    }

    #region Commands

    private async Task<ModbusData> ReadBlocks()
    {
        _log.Debug($"[{DeviceCode}] WagoController.ReadBlocks");
        foreach (var modbusBlock in Data.modbusBlocks)
        {
            //only input Blocks
            if (!modbusBlock.Value.isOutput)
            {
                await ReadBlock(modbusBlock.Key);
            }
        }
        return await Task.FromResult(Data);
    }

    private async Task<ModbusData> ReadBlock(string blockName)
    {
        await _slim.WaitAsync();
        _readAnswered = false;
        try
        {
            _log.Debug($"[{DeviceCode}] WagoController.ReadBlock {blockName}");
            Data.Command = ModbusCommands.ReadBlock.ToString();
            Data.ReadBlockName = blockName;  //for back writing in answer
            var tel = await RunTelegram(Data, Data.ReadBlockCommand(blockName));
            if (tel.Error != 0)
            {
                Data.ErrorNr = 99;
                Data.ErrorText = tel.ErrorText;
            }
        }
        finally
        {
            if (!_readAnswered)
            {
                _slim.Release();
            }
        }
        return await Task.FromResult(Data);
    }

    public async Task<ModbusData> WriteVariable(string variableName, string value)
    {
        // TODO: writing Bit of Register Block: other Bits from Data.Blocks

        await _slim.WaitAsync();
        _readAnswered = false;
        try
        {
            _log.Debug($"[{DeviceCode}] WagoController.WriteVariable");
            Data.Command = ModbusCommands.WriteVariable.ToString();
            var tel = await RunTelegram(Data, Data.WriteVariableCommand(variableName, value));
            if (tel.Error != 0)
            {
                Data.ErrorNr = 99;
                Data.ErrorText = tel.ErrorText;
            }
        }
        finally
        {
            if (!_readAnswered)
            {
                _slim.Release();
            }
        }
        return await Task.FromResult(Data);
    }

    #endregion

    #region Callbacks

    private void WagoControllerAnswer(ComTelegram tel)
    {
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(WagoControllerAnswer));
        var inBuff = tel.InData;
        //string inStr = Encoding.ASCII.GetString(_inBuff.Buff, 0, _inBuff.Cnt);
        if (Data.Command == ModbusCommands.ReadBlock.ToString())
        {
            //Data.Status = ModbusStatus.WeightOK;
            _log.Debug($"[{DeviceCode}] WagoController.Answer");

            //received Values to Block.data
            Data.SetBlockData(Data.ReadBlockName, inBuff);

        }
        if (!_readAnswered)
        {
            _readAnswered = true;
            _slim.Release();
        }
    }

    #endregion
}
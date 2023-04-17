namespace Quva.Services.Devices.Modbus;

public class WagoController : ComProtocol, IModbusApi
{
    public ModbusData Data { get; set; }
    private readonly DeviceOptions _deviceOptions;
    private bool readAnswered = false;

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
    }

    public async Task<ModbusData> ModbusCommand(string command, string variableName, string value)
    {
        if (Enum.TryParse(command, out ModbusCommands cmd))
        {
            var data = cmd switch
            {
                ModbusCommands.ReadBlocks => await ReadBlocks(),
                ModbusCommands.WriteVariable => await WriteVariable(variableName, value),
                _ => throw new NotImplementedException($"ModbusCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }

        throw new NotImplementedException($"ModbusCommand({command}) not implemented");
    }

    #region Commands

    public async Task<ModbusData> ReadBlocks()
    {
        _log.Debug($"[{DeviceCode}] WagoController.ReadBlocks");
        foreach (var modbusBlock in Data.modbusBlocks)
        {
            await ReadBlock(modbusBlock.Key);
        }
        return await Task.FromResult(Data);
    }

    public async Task<ModbusData> ReadBlock(string blockName)
    {
        //ComPort.SetParamString(Data.Url); //set _httpParameter.URL
        _log.Debug($"[{DeviceCode}] WagoController.ReadBlock {blockName}");
        var tel = await RunTelegram(Data, ModbusCommands.ReadBlock.ToString());
        if (tel.Error != 0)
        {
            Data.ErrorNr = 99;
            Data.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(Data);
    }

    public async Task<ModbusData> WriteVariable(string variableName, string value)
    {
        // TODO: writing Bit of Register Block: other Bits from Data.Blocks
        
        _log.Debug($"[{DeviceCode}] WagoController.WriteVariable");
        var tel = await RunTelegram(Data, Data.WriteCommand(variableName, value));
        if (tel.Error != 0)
        {
            Data.ErrorNr = 99;
            Data.ErrorText = tel.ErrorText;
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
        if (Data.Command == ModbusCommands.ReadBlocks.ToString())
        {
            //Data.Status = ModbusStatus.WeightOK;
            _log.Debug($"[{DeviceCode}] WagoController.Answer");

            //received Values to Block.data
            Data.SetBlockData(Data.ReadBlockName, inBuff);

        }
    }

    #endregion
}
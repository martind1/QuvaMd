using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Devices.Modbus;

public class WagoController : ComProtocol, IModbusApi
{
    public ModbusData Data { get; set; }
    private readonly DeviceOptions _deviceOptions;

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
        Data = new ModbusData(device.Code, ModbusCommands.ReadBlocks.ToString());

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
                _ => throw new NotImplementedException($"ModbusCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }

        throw new NotImplementedException($"ModbusCommand({command}) not implemented");
    }

    #region Commands

    public async Task<ModbusData> ReadBlocks()
    {

        //ComPort.SetParamString(Data.Url); //set _httpParameter.URL
        _log.Debug($"[{DeviceCode}] WagoController.ReadBlocks");
        var tel = await RunTelegram(Data, "ReadBlocks"); //no command to send
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
        }
    }

    #endregion
}
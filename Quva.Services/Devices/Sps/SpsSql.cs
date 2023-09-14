using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quva.Services.Devices.Sps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quva.Services.Devices.Sps;

// Polls Table Readorder per Sql
public class SpsSql : ComProtocol, ISpsApi
{
    private readonly DeviceOptions _deviceOptions;
    private readonly int _loadingPoint;  //1=LKW Waage, 2=Bahn, 3=Halde
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private SpsData _readData;

    public string[] SpsSqlDescription =
    {
        ";Automatische Erzeugung. Ändern nicht möglich.",
        "T:3000", //waiting 3 seconds for cam image
        "A:"
    };

    public SpsSql(ComDevice device, IServiceScopeFactory serviceScopeFactory) : base(device.Code, device.ComPort)
    {
        Description = SpsSqlDescription;
        OnAnswer = SpsSqlAnswer;
        _readData = new SpsData(device.Code, SpsCommands.Read.ToString());

        ArgumentNullException.ThrowIfNull(device.Options);
        _deviceOptions = device.Options;
        _loadingPoint = int.Parse(device.Device.Paramstring ?? "0");
        _serviceScopeFactory = serviceScopeFactory;
        PollInterval = _deviceOptions.Option("PollInterval", 10000);
    }

    public async Task<SpsData> SpsCommand(string command)
    {
        if (Enum.TryParse(command, out SpsCommands cmd))
        {
            var data = cmd switch
            {
                SpsCommands.Read => await Read(),
                _ => throw new NotImplementedException($"SpsCommand.{command} not implemented")
            };
            return await Task.FromResult(data);
        }

        throw new NotImplementedException($"SpsCommand({command}) not implemented");
    }

    #region Commands

    public async Task<SpsData> Read()
    {
        _readData = new SpsData(DeviceCode, SpsCommands.Read.ToString()) //important
        {
            Status = SpsStatus.Timeout,
            LoadingPoint = _loadingPoint
        };

        ComPort.SetParamString(_loadingPoint.ToString());
        ComPort.SetParameter(_serviceScopeFactory);

        _log.Debug($"[{DeviceCode}] SpsSql.Read({_readData.LoadingPoint})");
        var tel = await RunTelegram(_readData, "Read"); //no command to send
        if (tel.Error != 0)
        {
            _readData.ErrorNr = 99;
            _readData.ErrorText = tel.ErrorText;
        }
        return await Task.FromResult(_readData);
    }

    #endregion

    #region Callbacks

    private void SpsSqlAnswer(ComTelegram tel)
    {
        ArgumentNullException.ThrowIfNull(tel.AppData, nameof(SpsSqlAnswer));
        var inBuff = tel.InData;
        //string inStr = Encoding.ASCII.GetString(_inBuff.Buff, 0, _inBuff.Cnt);
        if (_readData.Command == SpsCommands.Read.ToString())
        {
            // inBuff.Buff -> json -> _readData
            if (inBuff.Cnt > 0)
            {
                string dataString = Encoding.UTF8.GetString(inBuff.Buff, 0, inBuff.Cnt);
                var inData = JsonSerializer.Deserialize<SpsData>(dataString);
                if (inData != null)
                {
                    inData.ToSpsData(_readData);
                    _readData.Status = SpsStatus.Ok;
                }
                else
                {
                    _readData.Status = SpsStatus.NotFound;
                }
            }
            else
            {
                _readData.Status = SpsStatus.NotFound;
            }
            _log.Debug($"[{DeviceCode}] SpsSql.Answer({_readData.LoadingPoint})");
        }
    }

    #endregion
}
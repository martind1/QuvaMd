using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement;
using Quva.Services.Devices;
using Quva.Services.Devices.Cam;
using Quva.Services.Devices.Card;
using Quva.Services.Devices.Display;
using Quva.Services.Devices.Modbus;
using Quva.Services.Devices.Scale;
using Quva.Services.Devices.Sps;
using static Quva.Services.Devices.ComDevice;

namespace Quva.Services.Interfaces.Shared
{
    public interface IDeviceService
    {
        Task<CamData> CamLoad(string devicecode, int camNumber);

        Task<CardData> CardRead(string devicecode);
        Task<IResult> CardReadStart(string devicecode, ComDevice.OnCardRead onCardRead);

        Task<DisplayData> DisplayShow(string devicecode, string message);
        Task<IResult> DisplayShowScale(string devicecode, string scaleCode);
        Task<IResult> DisplayShowStart(string devicecode, ComDevice.OnDisplayShow onDisplayShow);

        string GetModbusValue(string devicecode, string variableName);
        Task<ModbusData> ModbusReadStart(string devicecode, ComDevice.OnModbusRead? onModbusRead);
        Task<ModbusData> ModbusWrite(string devicecode, string variableName, string value);

        string GetScaleDisplay(string scaleCode);
        Task<ScaleData> ScaleRegister(string devicecode);
        Task<ScaleData> ScaleStatus(string devicecode);
        Task<IResult> ScaleStatusStart(string devicecode, ComDevice.OnScaleStatus onScaleStatus);

        Task<SpsData> SpsRead(string devicecode);
        Task<IResult> SpsReadStart(string devicecode, OnSpsRead onSpsRead);

        Task<ComDevice?> SimulCommandStart(string devicecode, ComProtocol.SimulDelegate onSimul);
        IServiceScopeFactory GetServiceScopeFactory();
        Task<DeviceDto?> GetDevice(string code);
        Task CloseDevice(string devicecode);
        ValueTask DisposeAsync();
    }

    public delegate Task ExecInContextFunction(QuvaContext context);
}
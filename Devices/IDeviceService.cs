using Microsoft.AspNetCore.Http;
using Quva.Devices.Cam;
using Quva.Devices.Card;
using Quva.Devices.Display;
using Quva.Devices.Scale;

namespace Quva.Devices
{
    public interface IDeviceService
    {
        Task<CardData> CardRead(string devicecode);
        Task<IResult> CardReadStart(string devicecode, ComDevice.OnCardRead onCardRead);

        Task<DisplayData> DisplayShow(string devicecode, string message);
        Task<IResult> DisplayShowScale(string devicecode, string scaleCode);
        Task<IResult> DisplayShowStart(string devicecode, ComDevice.OnDisplayShow onDisplayShow);

        string GetScaleDisplay(string scaleCode);
        Task<ScaleData> ScaleRegister(string devicecode);
        Task<ScaleData> ScaleStatus(string devicecode);
        Task<IResult> ScaleStatusStart(string devicecode, ComDevice.OnScaleStatus onScaleStatus);

        Task<CamData> CamLoad(string devicecode, int camNumber);

        Task CloseDevice(string devicecode);
        ValueTask DisposeAsync();
    }
}
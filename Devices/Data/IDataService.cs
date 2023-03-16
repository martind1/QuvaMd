namespace Quva.Devices.Data;

public interface IDataService
{
    Task<Device> GetDevice(string code);
}
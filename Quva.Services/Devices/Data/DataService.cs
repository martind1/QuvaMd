using Quva.Services.Devices.ComPort;
using System.Text.Json;

namespace Quva.Services.Devices.Data;

public class DataService : IDataService
{
    //private readonly MapperConfiguration mapperConfiguration;

    public async Task<Device> GetDevice(string code)
    {
        var fileName = @$"c:\Temp\ScaleData\{code}.json";
        var jsonString = await File.ReadAllTextAsync(fileName);
        var device = JsonSerializer.Deserialize<Device>(jsonString)!;

        return await Task.FromResult(device);
    }

}
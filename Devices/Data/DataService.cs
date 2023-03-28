using System.Text.Json;
using Devices.ComPort;

namespace Devices.Data;

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

    //Test: JSON erzeugen
    public void TestDevice()
    {
        var device = new Device
        {
            Id = 1,
            Code = "HOH.FW1",
            Name = "Test FawaWS",
            Roles = (int)(DeviceRoles.Entry | DeviceRoles.Exit),
            PortType = PortType.Tcp,
            ParamString = "127.0.0.1:12002",
            DeviceType = DeviceType.Scale,
            ModulCode = "FAWAWS",
            Transport = TransportType.Truck,
            Packaging = PackagingType.Bulk,
            Options = new Dictionary<string, string>
            {
                ["GerNr"] = "1",
                ["Unit"] = ScaleUnit.Ton.ToString()
            },
            Comment = "localer ShTcpSvr.exe"
        };
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(device, options);
        File.WriteAllText(@$"{device.Code}.json", jsonString);

        device = new Device
        {
            Id = 1,
            Code = "HOH.FW2",
            Name = "Test IT6000",
            Roles = (int)DeviceRoles.None,
            PortType = PortType.Tcp,
            ParamString = "10.171.149.31:1234",
            DeviceType = DeviceType.Scale,
            ModulCode = "IT6000",
            Transport = TransportType.Truck,
            Packaging = PackagingType.Bulk,
            Options = new Dictionary<string, string>
            {
                ["GerNr"] = "1",
                ["Unit"] = ScaleUnit.Ton.ToString()
            },
            Comment = "Original HOH IT6000 Scale"
        };
        options = new JsonSerializerOptions { WriteIndented = true };
        jsonString = JsonSerializer.Serialize(device, options);
        File.WriteAllText(@$"{device.Code}.json", jsonString);
    }
}
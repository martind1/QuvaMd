using AutoMapper;
using Microsoft.Extensions.Options;
using Quva.Devices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Devices.Data
{
    public class DataService
    {
        //private readonly MapperConfiguration mapperConfiguration;

        public DataService()
        {
            //mapperConfiguration = new MapperConfiguration(cfg => cfg.CreateMap<Device, ComDevice>());
        }

        public async Task<Device> GetDevice(string code)
        {
            string fileName = @$"Data\{code}.json";
            string jsonString = File.ReadAllText(fileName);
            Device device = JsonSerializer.Deserialize<Device>(jsonString)!;

            return await Task.FromResult(device);
        }

        //Test: JSON erzeugen
        public void TestDevice()
        {
            var device = new Device()
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
            string jsonString = JsonSerializer.Serialize(device, options);
            File.WriteAllText(@$"{device.Code}.json", jsonString);

            device = new Device()
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
}

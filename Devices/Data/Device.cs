using Quva.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Devices.Data;

public class Device
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public DeviceRoles Roles { get; set; }  //must bei int because Flags
    public string? ParamString { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DeviceType DeviceType { get; set; }

    public string? ModulCode { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransportType Transport { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PackagingType Packaging { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PortType PortType { get; set; }

    public Dictionary<string, string>? Options { get; set; }

    public string? Comment { get; set; }

}

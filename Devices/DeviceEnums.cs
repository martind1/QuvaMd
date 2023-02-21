using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quva.Devices;

public enum DeviceType
{
    None,
    Scale,
    Card,
    Display,
    Cam,
    SpsVisu
}

[Flags]
public enum DeviceRoles
{
    None = 0,
    Entry = 1,  //'E'
    Exit = 2,   //'A'
}

public enum TransportType
{
    All,
    Truck,
    Rail
}

public enum PackagingType
{
    All,
    Bulk,
    Packaged
}

public enum ScaleUnit
{
    Ton,
    Kilogram,
    Gram
}

﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class VDeviceParameter
{
    public string DevCode { get; set; } = null!;

    public string? DeviceTypeDisp { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public long Id { get; set; }

    public long IdDevice { get; set; }
}
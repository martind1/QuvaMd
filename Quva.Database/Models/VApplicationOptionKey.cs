﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class VApplicationOptionKey
{
    public string GroupName { get; set; } = null!;

    public string KeyName { get; set; } = null!;

    public string? TypeDisplay { get; set; }

    public string InitialValue { get; set; } = null!;

    public int Datatype { get; set; }

    public long IdKey { get; set; }

    public long IdGroup { get; set; }
}
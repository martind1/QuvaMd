﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class VLocationParameter
{
    public string LocShortname { get; set; } = null!;

    public string? GroupDotKey { get; set; }

    public string? PlaCode { get; set; }

    public string? TypeDisplay { get; set; }

    public string Value { get; set; } = null!;

    public string InitialValue { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public string KeyName { get; set; } = null!;

    public int Datatype { get; set; }

    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long? IdPlant { get; set; }

    public long IdKey { get; set; }

    public long IdGroup { get; set; }
}
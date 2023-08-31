﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class VCustomerAgreement
{
    public long Id { get; set; }

    public string Loc { get; set; } = null!;

    public string? Pla { get; set; }

    public string? Ctry { get; set; }

    public long? DebNo { get; set; }

    public string? DebName1 { get; set; }

    public string? Code { get; set; }

    public string Category { get; set; } = null!;

    public string? PckTyp { get; set; }

    public string? TrpTyp { get; set; }

    public bool Goods { get; set; }

    public bool Invoice { get; set; }

    public bool Carrier { get; set; }

    public bool Active { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public long IdLocation { get; set; }

    public long? IdPlant { get; set; }

    public long? IdCountry { get; set; }

    public long? IdDebitor { get; set; }

    public long? IdMaterial { get; set; }

    public int PackagingType { get; set; }

    public int TransportType { get; set; }

    public long IdCategory { get; set; }
}
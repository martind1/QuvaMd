﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class LoadorderSilo
{
    public long Id { get; set; }

    public long IdLoadorderHead { get; set; }

    public int SiloSet { get; set; }

    public int Position { get; set; }

    public long? IdSilo { get; set; }

    public int? SiloNumber { get; set; }

    public string? Akz { get; set; }

    public string? SpsCode { get; set; }

    public long? IdBasicType { get; set; }

    public decimal? SiloLevelVolume { get; set; }

    public decimal? Percentage { get; set; }

    public decimal? PowerTh { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long? IdContingentSilo { get; set; }

    public virtual BasicType? IdBasicTypeNavigation { get; set; }

    public virtual ContingentSilo? IdContingentSiloNavigation { get; set; }

    public virtual LoadorderHead IdLoadorderHeadNavigation { get; set; } = null!;

    public virtual Silo? IdSiloNavigation { get; set; }
}
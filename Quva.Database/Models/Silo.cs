﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class Silo
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public int SiloNumber { get; set; }

    public string? Akz { get; set; }

    public long IdLocation { get; set; }

    public long? IdBasicType { get; set; }

    public string? SpsCode { get; set; }

    public decimal? MaxPercentage { get; set; }

    public string? SpsBasicType { get; set; }

    public bool LockRail { get; set; }

    public bool LockTruck { get; set; }

    public bool LockForProduction { get; set; }

    public bool LockBigbag { get; set; }

    public int Priority { get; set; }

    public decimal? SiloVolume { get; set; }

    public decimal? SiloLevelVolume { get; set; }

    public decimal? SiloLevelPercentage { get; set; }

    public decimal? MinSiloLevelVolume { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public bool Dry { get; set; }

    public int SortNumber { get; set; }

    public string? DrainageTime { get; set; }

    public bool LockLaboratory { get; set; }

    public bool LockForSensitiveCustomer { get; set; }

    public virtual ICollection<AdditionalBasicType> AdditionalBasicType { get; set; } = new List<AdditionalBasicType>();

    public virtual ICollection<ContingentSilo> ContingentSilo { get; set; } = new List<ContingentSilo>();

    public virtual BasicType? IdBasicTypeNavigation { get; set; }

    public virtual Location IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<LoadorderSilo> LoadorderSilo { get; set; } = new List<LoadorderSilo>();

    public virtual ICollection<MappingSiloLoadingPoint> MappingSiloLoadingPoint { get; set; } = new List<MappingSiloLoadingPoint>();
}
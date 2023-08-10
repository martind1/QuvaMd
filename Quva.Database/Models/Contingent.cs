﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class Contingent
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long IdLoadingPoint { get; set; }

    public long? IdDebitor { get; set; }

    public bool Active { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public long IdSilo1 { get; set; }

    public decimal SiloPart1 { get; set; }

    public long? IdSilo2 { get; set; }

    public decimal? SiloPart2 { get; set; }

    public long? IdSilo3 { get; set; }

    public decimal? SiloPart3 { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdConfigPlantMaterial { get; set; }

    public virtual ConfigPlantMaterial IdConfigPlantMaterialNavigation { get; set; } = null!;

    public virtual Debitor? IdDebitorNavigation { get; set; }

    public virtual LoadingPoint IdLoadingPointNavigation { get; set; } = null!;

    public virtual Location IdLocationNavigation { get; set; } = null!;

    public virtual Silo IdSilo1Navigation { get; set; } = null!;

    public virtual Silo? IdSilo2Navigation { get; set; }

    public virtual Silo? IdSilo3Navigation { get; set; }
}
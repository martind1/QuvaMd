﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class IdentificationCard
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public long IdVehicles { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public bool Locked { get; set; }

    public bool FlagDispencer { get; set; }

    public long? IdDebitor { get; set; }

    public virtual Debitor? IdDebitorNavigation { get; set; }

    public virtual Vehicle IdVehiclesNavigation { get; set; } = null!;

    public virtual ICollection<MappingCarrierDebitor> MappingCarrierDebitor { get; set; } = new List<MappingCarrierDebitor>();
}
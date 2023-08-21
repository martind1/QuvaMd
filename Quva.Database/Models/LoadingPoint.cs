﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class LoadingPoint
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public int LoadingNumber { get; set; }

    public string Name { get; set; } = null!;

    public int PackagingType { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public int TransportType { get; set; }

    public virtual ICollection<Contingent> Contingent { get; set; } = new List<Contingent>();

    public virtual Location IdLocationNavigation { get; set; } = null!;

    public virtual ICollection<LoadorderHead> LoadorderHead { get; set; } = new List<LoadorderHead>();

    public virtual ICollection<MappingSiloLoadingPoint> MappingSiloLoadingPoint { get; set; } = new List<MappingSiloLoadingPoint>();
}
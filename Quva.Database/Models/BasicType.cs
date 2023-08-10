﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class BasicType
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public long IdMaterial { get; set; }

    public string? SpsCode { get; set; }

    public long? IdBasicType1 { get; set; }

    public int? IdBasicType1Percentage { get; set; }

    public long? IdBasicType2 { get; set; }

    public int? IdBasicType2Percentage { get; set; }

    public long? IdBasicType3 { get; set; }

    public int? IdBasicType3Percentage { get; set; }

    public long? IdBasicType4 { get; set; }

    public int? IdBasicType4Percentage { get; set; }

    public long? IdBasicType5 { get; set; }

    public int? IdBasicType5Percentage { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<AdditionalBasicType> AdditionalBasicType { get; set; } = new List<AdditionalBasicType>();

    public virtual BasicType? IdBasicType1Navigation { get; set; }

    public virtual BasicType? IdBasicType2Navigation { get; set; }

    public virtual BasicType? IdBasicType3Navigation { get; set; }

    public virtual BasicType? IdBasicType4Navigation { get; set; }

    public virtual BasicType? IdBasicType5Navigation { get; set; }

    public virtual Location IdLocationNavigation { get; set; } = null!;

    public virtual Material IdMaterialNavigation { get; set; } = null!;

    public virtual ICollection<BasicType> InverseIdBasicType1Navigation { get; set; } = new List<BasicType>();

    public virtual ICollection<BasicType> InverseIdBasicType2Navigation { get; set; } = new List<BasicType>();

    public virtual ICollection<BasicType> InverseIdBasicType3Navigation { get; set; } = new List<BasicType>();

    public virtual ICollection<BasicType> InverseIdBasicType4Navigation { get; set; } = new List<BasicType>();

    public virtual ICollection<BasicType> InverseIdBasicType5Navigation { get; set; } = new List<BasicType>();

    public virtual ICollection<Silo> Silo { get; set; } = new List<Silo>();
}
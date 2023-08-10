﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class Unit
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public string Display { get; set; } = null!;

    public virtual ICollection<Batch> Batch { get; set; } = new List<Batch>();

    public virtual ICollection<DeliveryOrderPosition> DeliveryOrderPosition { get; set; } = new List<DeliveryOrderPosition>();

    public virtual ICollection<Material> Material { get; set; } = new List<Material>();

    public virtual ICollection<OrderPosition> OrderPosition { get; set; } = new List<OrderPosition>();

    public virtual ICollection<QuantityUnit> QuantityUnit { get; set; } = new List<QuantityUnit>();
}
﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class OrderPosition
{
    public long Id { get; set; }

    public long IdOrderHead { get; set; }

    public int PositionNumber { get; set; }

    public long IdUnit { get; set; }

    public string? CustomerProductName { get; set; }

    public string MaterialShortName { get; set; } = null!;

    public string MaterialLongName { get; set; } = null!;

    public string MaterialName { get; set; } = null!;

    public bool MainPosition { get; set; }

    public long IdProductgroup { get; set; }

    public string ProductgroupName { get; set; } = null!;

    public bool Active { get; set; }

    public int? TopPosition { get; set; }

    public string? ShippingPoint { get; set; }

    public string? PlantCertificate { get; set; }

    public decimal OrderQuantity { get; set; }

    public decimal DeliveredQuantity { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public decimal ReservedQuantity { get; set; }

    public virtual OrderHead IdOrderHeadNavigation { get; set; } = null!;

    public virtual Productgroup IdProductgroupNavigation { get; set; } = null!;

    public virtual Unit IdUnitNavigation { get; set; } = null!;
}
﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class OdcAdjustmentQuantity
{
    public long Id { get; set; }

    public long IdOdcAdjustmentDay { get; set; }

    public long IdConfigPlantMaterial { get; set; }

    public int SpMark { get; set; }

    public int Transporttype { get; set; }

    public int Packagingtype { get; set; }

    public decimal Quantity { get; set; }

    public bool? DelFlag { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ConfigPlantMaterial IdConfigPlantMaterialNavigation { get; set; } = null!;

    public virtual OdcAdjustmentDay IdOdcAdjustmentDayNavigation { get; set; } = null!;
}
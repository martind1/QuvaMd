﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class LoadorderHead
{
    public long Id { get; set; }

    public long IdDelivery { get; set; }

    public int LoadorderState { get; set; }

    public long IdLoadingPoint { get; set; }

    public decimal? TargetQuantity { get; set; }

    public decimal? ActualQuantity { get; set; }

    public decimal? MaxGross { get; set; }

    public int? WeighingUnit { get; set; }

    public DateTime? TaraDate { get; set; }

    public string? TaraCalibNumber { get; set; }

    public decimal? TaraWeight { get; set; }

    public DateTime? GrossDate { get; set; }

    public string? GrossCalibNumber { get; set; }

    public decimal? GrossWeight { get; set; }

    public DateTime? NetDate { get; set; }

    public string? NetCalibNumber { get; set; }

    public decimal? NetWeight { get; set; }

    public string? SandCalibNumber { get; set; }

    public decimal? SandWeight { get; set; }

    public string? GravelCalibNumber { get; set; }

    public decimal? GravelWeight { get; set; }

    public string? DialogLanguage { get; set; }

    public string? StateText { get; set; }

    public string? ErrorMessages { get; set; }

    public string? ChargeNumber { get; set; }

    public bool? FlagManuell { get; set; }

    public bool? FlagOffeneBel { get; set; }

    public bool? FlagSpsBel { get; set; }

    public bool? FlagKombiKnz { get; set; }

    public bool? FlagQuittieren { get; set; }

    public bool? FlagProbenahme { get; set; }

    public bool? FlagPattern { get; set; }

    public bool? FlagAnalyse { get; set; }

    public bool? FlagAuftWdhlg { get; set; }

    public string? PatternNumber { get; set; }

    public bool? FlagNotbetrieb { get; set; }

    public bool? FlagVerladesperre { get; set; }

    public int? SilochangesCount { get; set; }

    public int? ActivePartNumber { get; set; }

    public decimal? MoistLock { get; set; }

    public decimal? MoistPercentage { get; set; }

    public DateTime? BeginDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual DeliveryHead IdDeliveryNavigation { get; set; } = null!;

    public virtual LoadingPoint IdLoadingPointNavigation { get; set; } = null!;

    public virtual ICollection<LoadorderPart> LoadorderPart { get; set; } = new List<LoadorderPart>();

    public virtual ICollection<LoadorderSilo> LoadorderSilo { get; set; } = new List<LoadorderSilo>();
}
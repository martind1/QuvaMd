﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class Deliveryheadview
{
    public long Id { get; set; }

    public long DeliveryNumber { get; set; }

    public DateTime? ExportFlagDateSap { get; set; }

    public int CountDeliveryPrint { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public DateTime? InitialWeighingDate { get; set; }

    public string? InitialWeighingCalibNumber { get; set; }

    public decimal? InitialWeighingWeight { get; set; }

    public int? InitialWeighingMode { get; set; }

    public int InitialWeighingUnit { get; set; }

    public string? InitialScaleCode { get; set; }

    public DateTime? FinalWeighingDate { get; set; }

    public string? FinalWeighingCalibNumber { get; set; }

    public decimal? FinalWeighingWeight { get; set; }

    public int? FinalWeighingMode { get; set; }

    public int FinalWeighingUnit { get; set; }

    public string? FinalScaleCode { get; set; }

    public decimal? NetWeight { get; set; }

    public int NetUnit { get; set; }

    public decimal? TaraWeight { get; set; }

    public int TaraUnit { get; set; }

    public decimal? GrossWeight { get; set; }

    public int GrossUnit { get; set; }

    public string? LoadingScaleCode { get; set; }

    public DateTime? StartLoadingDate { get; set; }

    public DateTime? EndLoadingDate { get; set; }

    public DateTime? ClosingDate { get; set; }

    public string? RailTerminal { get; set; }

    public string? ContainerNumber { get; set; }

    public long? LoadingSlip { get; set; }

    public decimal? MoistPercentage { get; set; }

    public string? IdentificationCardCode { get; set; }

    public int CumulativeFlag { get; set; }

    public long? IdDeliveryCollect { get; set; }

    public int DeliveryState { get; set; }

    public DateTime? CancelDate { get; set; }

    public string? VehicleNumber { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public int SapExportState { get; set; }

    public DateTime DeliveryDate { get; set; }

    public int InitialWeighingType { get; set; }

    public int FinalWeighingType { get; set; }

    public long? IdDeliveryOrder { get; set; }

    public long? OrderNumber { get; set; }

    public string? UnloadingPoint { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Customernumber { get; set; }

    public string? Customername1 { get; set; }

    public string? Customername2 { get; set; }

    public string? Customername3 { get; set; }

    public string? Customerstreet { get; set; }

    public string? Customerhousenumber { get; set; }

    public string? Customerpostcode { get; set; }

    public string? Customerlocation { get; set; }

    public string? Entrancetime { get; set; }

    public string? Leavetime { get; set; }

    public string? Shippingcode { get; set; }

    public string? Shippingname { get; set; }

    public int? Shippingpackagingtype { get; set; }

    public int? Shippingtransporttype { get; set; }

    public string? Carrier { get; set; }
}
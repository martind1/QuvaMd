﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class VDeliveryTransfer
{
    public long Id { get; set; }

    public long DeliveryNumber { get; set; }

    public long OrderNumber { get; set; }

    public string? TransferTypeDisp { get; set; }

    public string? SapExportStateDisp { get; set; }

    public DateTime TransferDate { get; set; }

    public bool ErrorState { get; set; }

    public string ErrorText { get; set; } = null!;

    public string? IdocType { get; set; }

    public string? IdocNumber { get; set; }

    public int TransferType { get; set; }

    public int SapExportState { get; set; }

    public long IdDelivery { get; set; }

    public long DelordId { get; set; }
}
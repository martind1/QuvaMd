﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class DeliveryTransfer
{
    public long Id { get; set; }

    public long IdDelivery { get; set; }

    public int TransferType { get; set; }

    public DateTime TransferDate { get; set; }

    public bool ErrorState { get; set; }

    public string ErrorText { get; set; } = null!;

    public string? IdocType { get; set; }

    public string? IdocNumber { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual DeliveryHead IdDeliveryNavigation { get; set; } = null!;
}
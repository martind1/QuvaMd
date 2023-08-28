﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class DeliveryMessage
{
    public long Id { get; set; }

    public long IdDeliveryHead { get; set; }

    public long IdConfigMessage { get; set; }

    public int MessageType { get; set; }

    public string? Subject { get; set; }

    public int State { get; set; }

    public DateTime? ToSendDate { get; set; }

    public DateTime? SendDate { get; set; }

    public string? SendError { get; set; }

    public string? SenderEmail { get; set; }

    public string? Recipient { get; set; }

    public string? Content { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<DeliveryDocument> DeliveryDocument { get; set; } = new List<DeliveryDocument>();

    public virtual ConfigMessage IdConfigMessageNavigation { get; set; } = null!;

    public virtual DeliveryHead IdDeliveryHeadNavigation { get; set; } = null!;
}
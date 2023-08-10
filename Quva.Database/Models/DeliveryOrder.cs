﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class DeliveryOrder
{
    public long Id { get; set; }

    public string? UnloadingPoint { get; set; }

    public bool ChangedOrder { get; set; }

    public string Art { get; set; } = null!;

    public long OrderNumber { get; set; }

    public string? FieldService { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? CustomerOrderNumber { get; set; }

    public DateTime? DesiredDeliveryDate { get; set; }

    public bool DeliveryLock { get; set; }

    public string? DeliveryCondition { get; set; }

    public string? DeliveryInfo { get; set; }

    public bool NonSapOrder { get; set; }

    public bool NewOrder { get; set; }

    public string? Clerk { get; set; }

    public string? PrintLanguage { get; set; }

    public bool Active { get; set; }

    public long IdShippingMethod { get; set; }

    public long IdPlant { get; set; }

    public int? RetrievalCode { get; set; }

    public bool Locked { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdDeliveryHead { get; set; }

    public long? IdSalesOrganization { get; set; }

    public virtual ICollection<DeliveryOrderDebitor> DeliveryOrderDebitor { get; set; } = new List<DeliveryOrderDebitor>();

    public virtual ICollection<DeliveryOrderPosition> DeliveryOrderPosition { get; set; } = new List<DeliveryOrderPosition>();

    public virtual ICollection<DeliveryOrderText> DeliveryOrderText { get; set; } = new List<DeliveryOrderText>();

    public virtual DeliveryHead IdDeliveryHeadNavigation { get; set; } = null!;

    public virtual Plant IdPlantNavigation { get; set; } = null!;

    public virtual SalesOrganization? IdSalesOrganizationNavigation { get; set; }

    public virtual ShippingMethod IdShippingMethodNavigation { get; set; } = null!;
}
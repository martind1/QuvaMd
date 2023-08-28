﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class MappingSoDebitor
{
    public long Id { get; set; }

    public long IdDebitor { get; set; }

    public long IdSalesOrganization { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public int? CountDeliveryPrint { get; set; }

    public virtual Debitor IdDebitorNavigation { get; set; } = null!;

    public virtual SalesOrganization IdSalesOrganizationNavigation { get; set; } = null!;
}
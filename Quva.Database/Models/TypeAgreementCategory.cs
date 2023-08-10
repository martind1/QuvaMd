﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class TypeAgreementCategory
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string Display { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<CustomerAgreement> CustomerAgreement { get; set; } = new List<CustomerAgreement>();

    public virtual ICollection<MappingAgrCategoryOption> MappingAgrCategoryOption { get; set; } = new List<MappingAgrCategoryOption>();
}
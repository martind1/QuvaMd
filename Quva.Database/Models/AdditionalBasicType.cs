﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class AdditionalBasicType
{
    public long Id { get; set; }

    public long IdBasicType { get; set; }

    public long IdSilo { get; set; }

    public int Priority { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual BasicType IdBasicTypeNavigation { get; set; } = null!;

    public virtual Silo IdSiloNavigation { get; set; } = null!;
}
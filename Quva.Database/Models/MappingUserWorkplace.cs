﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class MappingUserWorkplace
{
    public long Id { get; set; }

    public long IdUser { get; set; }

    public long IdWorkplace { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual Users IdUserNavigation { get; set; } = null!;

    public virtual Workplace IdWorkplaceNavigation { get; set; } = null!;
}
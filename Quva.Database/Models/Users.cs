﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class Users
{
    public long Id { get; set; }

    public string AdAccount { get; set; } = null!;

    public DateTime? LastLoginDate { get; set; }

    public string Name { get; set; } = null!;

    public bool Active { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<MappingUserWorkplace> MappingUserWorkplace { get; set; } = new List<MappingUserWorkplace>();
}
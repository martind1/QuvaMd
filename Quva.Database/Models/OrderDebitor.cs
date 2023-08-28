﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class OrderDebitor
{
    public long Id { get; set; }

    public long IdOrder { get; set; }

    public long IdCountry { get; set; }

    public int Role { get; set; }

    public string Name1 { get; set; } = null!;

    public string? Name2 { get; set; }

    public string? Name3 { get; set; }

    public string? Street { get; set; }

    public string? Housenumber { get; set; }

    public string? Postcode { get; set; }

    public string? Location { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Language { get; set; }

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long DebitorNumber { get; set; }

    public virtual Country IdCountryNavigation { get; set; } = null!;

    public virtual OrderHead IdOrderNavigation { get; set; } = null!;
}
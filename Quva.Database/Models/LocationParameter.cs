﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class LocationParameter
{
    public long Id { get; set; }

    public long IdLocation { get; set; }

    public string Value { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public long IdOptionKey { get; set; }

    public long? IdPlant { get; set; }

    public virtual Location IdLocationNavigation { get; set; } = null!;

    public virtual TypeApplicationOptionKey IdOptionKeyNavigation { get; set; } = null!;

    public virtual Plant? IdPlantNavigation { get; set; }
}
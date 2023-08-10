﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class ConfigProductgroup
{
    public long Id { get; set; }

    public long IdPlant { get; set; }

    public long IdProductgroup { get; set; }

    public string GroupKey { get; set; } = null!;

    public string MaterialKind { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual Plant IdPlantNavigation { get; set; } = null!;

    public virtual Productgroup IdProductgroupNavigation { get; set; } = null!;
}
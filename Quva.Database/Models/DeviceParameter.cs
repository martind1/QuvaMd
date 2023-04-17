﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class DeviceParameter
{
    public long Id { get; set; }

    public long IdDevice { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public string? Description { get; set; }

    public virtual Device IdDeviceNavigation { get; set; } = null!;
}
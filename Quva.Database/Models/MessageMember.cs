﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;

namespace Quva.Database.Models;

public partial class MessageMember
{
    public long Id { get; set; }

    public long IdMessageDispatcher { get; set; }

    public int MemberType { get; set; }

    public string Adress { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CreateUser { get; set; } = null!;

    public DateTime CreateDate { get; set; }

    public string? ChangeUser { get; set; }

    public DateTime? ChangeDate { get; set; }

    public int ChangeNumber { get; set; }

    public string? Note { get; set; }

    public virtual MessageDispatcher IdMessageDispatcherNavigation { get; set; } = null!;
}
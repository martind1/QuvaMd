﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace DeviceSimulator.Models
{
    public partial class DeviceParameter
    {
        public long Id { get; set; }
        public long? IdDevice { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string ChangeUser { get; set; }
        public string ChangeDate { get; set; }
        public long? ChangeNumber { get; set; }
        public string Note { get; set; }

        public virtual Device IdDeviceNavigation { get; set; }
    }
}
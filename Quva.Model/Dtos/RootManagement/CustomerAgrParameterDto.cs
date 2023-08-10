﻿using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Model.Dtos.RootManagement
{
    public class CustomerAgrParameterDto : BaseDto<CustomerAgrParameterDto, CustomerAgrParameter>
    {
        public long Id { get; set; }

        public long IdAgreement { get; set; }

        public long IdOption { get; set; }

        public string ParameterValue { get; set; } = null!;

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public virtual CustomerAgreementDto? IdAgreementNavigation { get; set; } = null!;

        public virtual TypeAgreementOptionDto? IdOptionNavigation { get; set; } = null!;
    }
}

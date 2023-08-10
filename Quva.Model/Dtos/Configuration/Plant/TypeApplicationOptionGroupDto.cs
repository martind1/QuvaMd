using Quva.Database.Models;
using Quva.Model.Dtos.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Model.Dtos.Configuration.Plant
{
    public class TypeApplicationOptionGroupDto : BaseDto<TypeApplicationOptionGroupDto, TypeApplicationOptionGroup>
    {
        public long Id { get; set; }

        public string GroupName { get; set; } = null!;

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public virtual ICollection<TypeApplicationOptionKeyDto>? TypeApplicationOptionKey { get; set; }
    }
}

using Quva.Database.Models;
using Quva.Model.Dtos.RootManagement;
using Quva.Model.Dtos.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Model.Dtos.Configuration.Plant
{
    public class TypeApplicationOptionKeyDto : BaseDto<TypeApplicationOptionKeyDto, TypeApplicationOptionKey>
    {
        public long Id { get; set; }

        public long IdGroup { get; set; }

        public string KeyName { get; set; } = null!;

        public string CreateUser { get; set; } = null!;

        public DateTime CreateDate { get; set; }

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int ChangeNumber { get; set; }

        public string? Note { get; set; }

        public string InitialValue { get; set; } = null!;

        public virtual TypeApplicationOptionGroupDto? IdGroupNavigation { get; set; } = null!;

        public virtual ICollection<LocationParameterDto>? LocationParameter { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Loading;

public record BaseTypeSiloFilter
{
    // Filterwerte
    public long? idDebitor { get; set; } = null;
    public long? idMaterial { get; set; } = null;
    public List<long> idLoadingPoints { get; set; } = new();
    public bool ContingentRequired { get; set; } = false;
}


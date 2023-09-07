using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Loading;

public record LoadingInfo
{
    public decimal MaxGross { get; set; } = 0;
    public decimal MaxNet { get; set; } = 0;
    public bool CumulativeFlag { get; set; } = false;

    public decimal MaxReloadWeight { get; set; } = 0;
}

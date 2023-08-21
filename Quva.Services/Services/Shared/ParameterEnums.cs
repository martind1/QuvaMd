using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Services.Shared;

public enum TransportType
{
    All = 0,
    Truck = 1,
    Rail = 2
}

public enum PackagingType
{
    All = 0,
    Bulk = 1,
    Packaged = 2
}

public enum DataTypeValues
{
    t_string = 0,
    t_int = 1,
    t_bool = 2,
    t_float = 3,
    t_date = 4,
    t_blob = 5
}

public enum OrderDebitorRole
{
    GoodsRecipient = 0,
    InvoiceRecipient = 1,
    ForwardingAgent = 2
}


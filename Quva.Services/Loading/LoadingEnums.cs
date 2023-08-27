using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Loading;

public enum LoadorderStateValues
{
    Inserted = 0,
    ToLoad = 1,
    LoadingRunning = 2,
    LoadingFinished = 3,
    LoadingCanceled = 4,
    ToReload = 5,
    GetWeight = 6,
    Inactive = 9
}

public enum WeighingUnit
{
Nothing = 0,
Ton = 1,
Kilogram = 2,
Gram = 3
}

public enum OrderDebitorRole
{
    GoodsRecipient = 0,
    InvoiceRecipient = 1,
    ForwardingAgent = 2
}

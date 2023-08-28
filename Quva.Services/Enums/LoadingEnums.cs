namespace Quva.Services.Enums;

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

public enum DeliveryStateValues
{
    //0 Entered, 1 Scheduled, 2 Yard list, 3 Loaded, 4 Weighed, 5 Exported SAP, 6 Cancelled.
    Entered = 0, 
    Scheduled = 1, 
    YardList = 2, 
    Loaded = 3, 
    Weighed = 4, 
    ExportedSap = 5, 
    Cancelled = 6
}
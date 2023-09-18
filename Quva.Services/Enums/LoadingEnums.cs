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
    Reserved1 = 7,
    Reserved2 = 8,
    Inactive = 9
}

public enum WeighingUnit
{
    Nothing = 0,
    Ton = 1,
    Kilogram = 2,
    Gram = 3
}

public enum LockRoleValues
{
    None = 0,
    BigBag = 1,
    Truck2 = 2,
}
using Quva.Services.Enums;

namespace Quva.Services.Loading.Helper;

// Regel mit Soll und Istwerten zum prüfen
public record RuleToApply
{
    public string Name { get; set; } = string.Empty;
    public bool CheckSiloLock { get; set; } = false;  //Truck, Rail, BigBag
    public bool CheckLaboratory { get; set; } = false;
    public bool CheckProduction { get; set; } = false;
    public bool CheckForSensitiveCustomer { get; set; } = false;
    public bool CheckSilolevel { get; set; } = false;

    // Istwerte von Lieferschein:
    public TransportTypeValues TransportType = TransportTypeValues.All;
    public decimal LoadingQuantity { get; set; } = 0;
    public bool SensitiveCustomer { get; set; } = false;
    public LockRoleValues LockRole { get; set; } = 0;
}


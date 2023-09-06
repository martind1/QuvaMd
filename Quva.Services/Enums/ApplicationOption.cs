namespace Quva.Services.Enums;

public static class ApplicationOption
{
    public record DeliveryExport
    {
        public const string CountOfDays = "DeliveryExport.CountOfDays";
        public const string ExportBefore = "DeliveryExport.ExportBefore";
        public const string ExcludeArts = "DeliveryExport.ExcludeArts";
    }

    public record WeighingMode  // Wägebetrieb
    {
        public const string CountOfDeliverynotes = "WeighingMode.CountOfDeliverynotes"; // Anzahl Lieferscheine (Vorgabe)
        public const string MaxGross = "WeighingMode.MaxGross"; // Max. Bruttogewicht für Lieferschein.
        public const string MaxSingleQuantity = "WeighingMode.MaxSingleQuantity";
    }
}
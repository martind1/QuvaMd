namespace Quva.Services.Loading.Helper;

public record LoadingParameter
{
    // Transfer between UI and Service: UI -> Service
    public long IdLocation { get; set; }
    public long IdDelivery { get; set; }
    public decimal TargetQuantity { get; set; }
    public List<decimal> PartQuantities { get; set; } = new();
    public bool OrderRepetition { get; set; } = false;

    //optional nach Silowechsel:
    public List<SiloSet> SiloSets { get; set; } = new();
}



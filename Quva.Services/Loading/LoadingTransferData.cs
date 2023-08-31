﻿namespace Quva.Services.Loading;

public class LoadingParameter
{
    // Transfer between UI and Service: UI -> Service
    public long IdLocation { get; set; }
    public long IdDelivery { get; set; }
    public decimal TargetQuantity { get; set; }
    public List<decimal> PartQuantities { get; set; } = new();

    //optional nach Silowechsel:
    public SiloSet? SiloSet { get; set; }
}


public class LoadingResult
{
    // Transfer between UI and Service: Service -> UI

    // Count=0 when no Loadorder created 
    public List<long> IdLoadorders { get; set; } = new();

    // Error Text when no Loadorder created
    // Warnings when Loadorder created
    public List<string> ErrorLines { get; set; } = new();


    public void AddErrorLines(List<string> otherLines)
    {
        ErrorLines.AddRange(otherLines);
    }
}


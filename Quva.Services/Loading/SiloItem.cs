using Quva.Database.Models;

namespace Quva.Services.Loading;

public record SiloItem
{
    // Verwaltung eines Silos einer Kombination
    public int Position { get; set; }  // ab 1
    public ContingentSilo? TheContingentSilo { get; set; }
    public Silo? TheSilo { get; set; }
    private decimal _percentage = 0;
    public decimal Percentage
    {
        get => _percentage;
        set => _percentage = TheSilo != null
                ? TheSilo.MaxPercentage is null or 0
                    ? Math.Min(value, 100)
                    : Math.Min((decimal)TheSilo.MaxPercentage, value)
                : value;
    }
    public decimal PowerTh { get; set; } = 0;  // in t/h, Haltern

    public virtual bool Equals(SiloItem? other)
    {
        if (TheSilo == null || other == null || other.TheSilo == null) return false;

        return TheSilo.SiloNumber == other.TheSilo.SiloNumber;
    }

    public override int GetHashCode() => TheSilo!.GetHashCode();
}


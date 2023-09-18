namespace Quva.Services.Loading.Helper;

public record LoadingInfo
{
    public decimal MaxGross { get; set; } = 0;
    public decimal MaxNet { get; set; } = 0;
    public bool CumulativeFlag { get; set; } = false;

    public decimal MaxReloadWeight { get; set; } = 0;
}

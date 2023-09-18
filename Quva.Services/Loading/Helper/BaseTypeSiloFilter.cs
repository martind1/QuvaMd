namespace Quva.Services.Loading.Helper;

public record BaseTypeSiloFilter
{
    // Filterwerte
    public long IdLocation { get; set; } = 0;
    public long? idDebitor { get; set; } = null;
    public long? idMaterial { get; set; } = null;
    public List<long> idLoadingPoints { get; set; } = new();
    public bool ContingentRequired { get; set; } = false;


}


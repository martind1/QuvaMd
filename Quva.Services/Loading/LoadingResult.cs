namespace Quva.Services.Loading;

public record LoadingResult
{
    // Transfer between UI and Service: Service -> UI

    // Count=0 when no Loadorder created 
    public List<long> IdLoadorders { get; set; } = new();

    // Error Text when no Loadorder created
    // Warnings when Loadorder created
    public List<string> ErrorLines { get; set; } = new();

    // Angezeigte Beladestelle(n):
    public List<string> LoadingPoints { get; set; } = new();


    public void AddErrorLines(List<string> otherLines)
    {
        ErrorLines.AddRange(otherLines);
    }
}



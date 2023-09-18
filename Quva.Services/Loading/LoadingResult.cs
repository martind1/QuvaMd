using Quva.Services.Enums;

namespace Quva.Services.Loading;

public record LoadingResult
{
    // Transfer between UI and Service: Service -> UI

    // Count=0 when no Loadorder created 
    public List<long> IdLoadorders { get; set; } = new();

    // Error Text when no Loadorder created
    // Warnings when Loadorder created
    public List<ErrorLine> ErrorLines { get; set; } = new();

    // Angezeigte Beladestelle(n):
    public List<string> LoadingPoints { get; set; } = new();


    public void AddErrorLines(List<ErrorLine> otherLines)
    {
        ErrorLines.AddRange(otherLines);
    }

    public List<string> ErrorStringList(LanguageEnum language)
    {
        List<string> result = new();
        foreach (var err in ErrorLines)
        {
            result.Add(err.ToString(language));
        }
        return result;
    }

}

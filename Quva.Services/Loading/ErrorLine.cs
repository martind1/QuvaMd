using Quva.Services.Enums;

namespace Quva.Services.Loading;

public record ErrorLine
{
    public string Code { get; set; }
    public object[] Parameter;

    public ErrorLine(string code, params object[] parameter)
    {
        Code = code;
        Parameter = parameter;
    }

    // Test:
    public string ToString(LanguageEnum language)
    {
        return string.Format(TrText.Tr(language, Code), Parameter);
    }
}
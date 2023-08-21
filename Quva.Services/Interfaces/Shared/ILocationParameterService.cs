namespace Quva.Services.Interfaces.Shared;

public interface ILocationParameterService
{
    Task<object> GetParameter(long idLocation, string groupDotKey, long? idPlant);
}
using System.Net;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IApiOperationCompletionService
{
    void HandleResponse(
        HttpMethod method,
        HttpStatusCode statusCode,
        string? uri = null,
        string? message = null);
}

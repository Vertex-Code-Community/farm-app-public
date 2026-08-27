using System.Net;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Shared.Constants.Snackbar;

namespace FarmApp.AdminComponents.Services;

internal class ApiOperationCompletionService(ISnackbarService snackbarService)
    : IApiOperationCompletionService
{
    public void HandleResponse(
        HttpMethod method,
        HttpStatusCode statusCode,
        string? uri = null,
        string? message = null)
    {
        var code = (int)statusCode;

        if (IsSuccessStatusCode(code))
        {
            HandleSuccessResponse(method, message, uri);
            return;
        }

        if (IsClientErrorStatusCode(code))
        {
            HandleClientError(statusCode, message);
            return;
        }

        if (IsServerErrorStatusCode(code))
        {
            HandleServerError(statusCode, message);
            return;
        }

        snackbarService.Show(message ?? "Unknown error.", SnackbarColors.Info);
    }

    private void HandleSuccessResponse(
        HttpMethod method,
        string? message,
        string? uri)
    {
        if (IsLoginRequest(uri))
        {
            return;
        }

        var successMessage = message ?? GetDefaultSuccessMessage(method);
        if (!string.IsNullOrEmpty(successMessage))
        {
            snackbarService.Show(successMessage, SnackbarColors.Success);
            return;
        }
    }

    private static bool IsLoginRequest(string? uri)
    {
        return !string.IsNullOrEmpty(uri) && uri.Contains("/login");
    }

    private void HandleClientError(HttpStatusCode statusCode, string? message)
    {
        var errorMessage = message ?? GetClientErrorMessage(statusCode);
        snackbarService.Show(errorMessage, SnackbarColors.Warning);
    }

    private void HandleServerError(HttpStatusCode statusCode, string? message)
    {
        var errorMessage = message ?? GetServerErrorMessage(statusCode);
        snackbarService.Show(errorMessage, SnackbarColors.Error);
    }

    private static bool IsSuccessStatusCode(int code) => code >= 200 && code < 300;

    private static bool IsClientErrorStatusCode(int code) => code >= 400 && code < 500;

    private static bool IsServerErrorStatusCode(int code) => code >= 500;

    private static string GetDefaultSuccessMessage(HttpMethod method)
    {
        return method.Method switch
        {
            "GET" => string.Empty,
            "POST" => "Operation completed successfully.",
            "PUT" or "PATCH" => "Changes saved successfully.",
            "DELETE" => "Resource deleted successfully.",
            _ => string.Empty
        };
    }

    private static string GetClientErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Invalid request.",
            HttpStatusCode.Unauthorized => "Authorization required.",
            HttpStatusCode.Forbidden => "Access denied.",
            HttpStatusCode.NotFound => "Resource not found.",
            HttpStatusCode.Conflict => "Data conflict.",
            HttpStatusCode.UnprocessableEntity => "Cannot process data.",
            _ => "Request error."
        };
    }

    private static string GetServerErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.InternalServerError => "Internal server error.",
            HttpStatusCode.BadGateway => "Bad gateway.",
            HttpStatusCode.ServiceUnavailable => "Service is unavailable.",
            HttpStatusCode.GatewayTimeout => "Gateway timeout.",
            _ => "Server error."
        };
    }
}

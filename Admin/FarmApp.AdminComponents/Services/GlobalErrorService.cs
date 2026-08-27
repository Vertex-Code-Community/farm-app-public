using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models;

namespace FarmApp.AdminComponents.Services;

public class GlobalErrorService
    : IGlobalErrorService
{
    public event Action<string>? OnErrorOccurred;

    public void ShowError(string errorMessage)
    {
        OnErrorOccurred?.Invoke(errorMessage);
    }

    public void ShowError(ApiResponse apiResponse)
    {
        var errorMessage = !string.IsNullOrEmpty(apiResponse.Message) ? apiResponse.Message : "Unknown error occurred";
        OnErrorOccurred?.Invoke(errorMessage);
    }

    public void HideError()
    {
        OnErrorOccurred?.Invoke(string.Empty);
    }
}

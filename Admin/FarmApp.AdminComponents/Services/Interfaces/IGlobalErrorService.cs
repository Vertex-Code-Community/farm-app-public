using FarmApp.Models;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IGlobalErrorService
{
    event Action<string>? OnErrorOccurred;
    void ShowError(string errorMessage);
    void ShowError(ApiResponse apiResponse);
    void HideError();
}

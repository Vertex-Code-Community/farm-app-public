using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.GlobalError;

public partial class GlobalErrorComponent
    : ComponentBase, IDisposable
{
    [Inject] public required IGlobalErrorService GlobalErrorService { get; set; }

    private string _errorMessage = string.Empty;
    private string? _errorTitle;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            GlobalErrorService.OnErrorOccurred += OnErrorOccurred;
        }
    }

    private void OnErrorOccurred(string errorMessage)
    {
        _errorMessage = errorMessage;
        _errorTitle = "Error";
        InvokeAsync(() =>
        {
            StateHasChanged();
            return Task.CompletedTask;
        });
    }

    private void CloseError()
    {
        _errorMessage = string.Empty;
        GlobalErrorService.HideError();
        StateHasChanged();
    }

    public void Dispose()
    {
        GlobalErrorService.OnErrorOccurred -= OnErrorOccurred;
    }
}

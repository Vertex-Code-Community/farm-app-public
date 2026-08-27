namespace FarmApp.AdminComponents.ComponentsCommon.Snackbar;

public partial class SnackbarHost
{
    protected override void OnInitialized()
    {
        SnackbarService.OnChanged += StateHasChanged;
    }

    public void Dispose()
    {
        SnackbarService.OnChanged -= StateHasChanged;
    }
}

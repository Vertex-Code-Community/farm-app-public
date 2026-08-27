using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;
namespace FarmApp.Components.Components.GlobalLoader;

public partial class GlobalLoaderComponent : IDisposable
{
    [Inject] private IGlobalLoaderService GlobalLoaderService { get; set; } = null!;
    [Inject] private INotificationService NotificationService { get; set; } = null!;
    [Inject] private IBackButtonService BackButtonService { get; set; } = null!;

    protected override void OnInitialized()
    {
        GlobalLoaderService.OnLoaderSwitch += StateHasChanged;
        NotificationService.OnUpdate += StateHasChanged;
        BackButtonService.OnBackButtonPressed += HandleBackButtonClick;
    }

    private Task<bool> HandleBackButtonClick()
    {
        if (GlobalLoaderService.IsLoading)
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public void Dispose()
    {
        BackButtonService.OnBackButtonPressed -= HandleBackButtonClick;
        GlobalLoaderService.OnLoaderSwitch -= StateHasChanged;
        NotificationService.OnUpdate -= StateHasChanged;
    }
}
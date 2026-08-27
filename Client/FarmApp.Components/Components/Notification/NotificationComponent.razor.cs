using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.Notification;

public partial class NotificationComponent : IDisposable
{
    [Inject] public required INotificationService NotificationService { get; set; }

    [Inject] public required IJSRuntime JsRuntime { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; }

    protected override void OnInitialized()
    {
        NotificationService.OnUpdate += StateHasChanged;
    }

    public void Dispose()
    {
        NotificationService.OnUpdate -= StateHasChanged;
    }
}
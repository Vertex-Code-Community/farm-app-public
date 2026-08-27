using Microsoft.AspNetCore.Components;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Services.Providers;
using Microsoft.JSInterop;

namespace FarmApp.Mobile;

public partial class Main : IDisposable
{
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required IJSRuntime JSRuntime { get; set; }
    [Inject] private INavigationService NavigationService { get; set; } = null!;
    [Inject] private IMapSteadService MapSteadService { get; set; } = null!;
    [Inject] private INotificationService NotificationService { get; set; } = null!;
    [Inject] private IEnumerable<IMapService> MapServices { get; set; } = null!;
    [Inject] private IMapModalService MapModalService { get; set; } = null!;
    [Inject] private IGlobalLoaderService GlobalLoaderService { get; set; } = null!;

    public static event Func<Task>? OnBlazorInitialized;
    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.OnSignIn += HandleAuthChanged;

        await AuthStateProvider.InitializeAsync();
        
        ServiceLocator.Register(NavigationService);
        ServiceLocator.Register(MapSteadService);
        ServiceLocator.Register(NotificationService);
        ServiceLocator.Register(MapModalService);
        ServiceLocator.Register(GlobalLoaderService);
        
        foreach (var service in MapServices)
            service.OnBlazorServicesLoaded();
        
        OnBlazorInitialized?.Invoke();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await JSRuntime.InvokeVoidAsync("hideStartupLoader");
    }
    private void HandleAuthChanged(bool _)
    {
        InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        AuthStateProvider.OnSignIn -= HandleAuthChanged;
    }
}
using System.Globalization;
using Microsoft.AspNetCore.Components;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Properties;

namespace FarmApp.Components.Pages.Main.Modals.MapProperty;

public partial class MapPropertyComponent : IDisposable
{
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required IPropertyService PropertyService { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; } 
    [Inject] public required IMapModalService MapModalService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; } 
    
    [Parameter] public PropertyViewModel Property { get; set; } = new();
    
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private bool _showRemovePropertyDialog = false;
    
    protected override void OnInitialized()
    {
        IMapCallbackService.OnRequestMapEventPermission += AllowMapEvents;
    }

    public void Dispose()
    {
        IMapCallbackService.OnRequestMapEventPermission -= AllowMapEvents;
    }
    
    private async Task OnRemovePropertyAsync()
    {
        _showRemovePropertyDialog = false;
        StateHasChanged();
        
        using var loader = GlobalLoaderService.SwitchOn();
        var deleteTask = PropertyService.DeleteAsync(Property.Id);
        
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        await Task.WhenAll(deleteTask, minimalTimeTask);

        var status = deleteTask.Result;
        
        if (!status) return;

        StateService.DeleteProperty(Property.Id);
        IMapPropertyService.InvokeRemoveProperty(Property.Id);
        MapModalService.Hide();
    }
    
    private void OnPropertyCalendarClicked()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Console.WriteLine($"TEST OnPropertyCalendarClicked {timestamp}");
        NavigationService.NavigateTo(Constants.ClientRoutes.PropertyCalendarPage,
            new Dictionary<string, object>
            {
                { "PropertyId", Property.Id }
            });
    }
    
    private void OnPropertyCalendarClicked(DateTime dateTime)
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.PropertyCalendarPage,
            new Dictionary<string, object>
            {
                { "PropertyId", Property.Id },
                { "Date", dateTime }
            });
    }
    
    private bool AllowMapEvents()
    {
        return !_showRemovePropertyDialog;
    }
}
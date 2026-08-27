using System.Globalization;
using Microsoft.AspNetCore.Components;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.Components.Pages.Main.Modals.MapCustomStead;

public partial class MapCustomSteadComponent : IDisposable
{
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required  ICustomSteadService CustomSteadService { get; set; }
    [Inject] public required  IMapModalService MapModalService { get; set; }
    [Inject] public required  IGlobalLoaderService GlobalLoaderService { get; set; }
    
    [Parameter] public CustomSteadModel CustomStead { get; set; } = new();
    [Parameter] public double Area { get; set; }
    
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private bool _showRemoveCustomSteadDialog = false;
    
    protected override void OnInitialized()
    {
        IMapCallbackService.OnRequestMapEventPermission += AllowMapEvents;
    }

    public void Dispose()
    {
        IMapCallbackService.OnRequestMapEventPermission -= AllowMapEvents;
    }
    
    private async Task OnRemoveCustomSteadAsync()
    {
        _showRemoveCustomSteadDialog = false;
        StateHasChanged();
        
        using var loader = GlobalLoaderService.SwitchOn();
        var success = await CustomSteadService.DeleteAsync(CustomStead.Id);
        if (!success) return;
        
        IMapSteadService.InvokeRemoveCustomStead(CustomStead.Id);
        MapModalService.Hide();
    }
    
    private bool AllowMapEvents()
    {
        return !_showRemoveCustomSteadDialog;
    }
}
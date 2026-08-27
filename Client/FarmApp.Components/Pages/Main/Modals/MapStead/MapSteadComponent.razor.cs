using System.Globalization;
using Microsoft.AspNetCore.Components;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Steads;

namespace FarmApp.Components.Pages.Main.Modals.MapStead;

public partial class MapSteadComponent
{
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required IMapModalService MapModalService { get; set; }
    
    [Parameter] public SteadModel Stead { get; set; } = new();
    
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
}
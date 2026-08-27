using System.Globalization;
using Microsoft.AspNetCore.Components;
using Bch.Components.Modal;
using Bch.Modules.Maths.Models;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Attributes;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Extensions;

namespace FarmApp.Components.Pages.Main.MapModal;

public partial class MapModalComponent : IDisposable
{
    [Inject] public required IMapModalService MapModalService { get; set; }
    [Parameter] public required RenderFragment<MapModalType?> ChildContent { get; set; }
    
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private readonly Vec2 _dimensions = new();
    private BchModal? _bchModal;
    private int _arrowRectDepth = 0;
    private int _arrowRectLength = 0;

    private int ArrowWidth => MapModalService.ArrowMode > 1 ? _arrowRectDepth : _arrowRectLength;
    private int ArrowHeight => MapModalService.ArrowMode > 1 ? _arrowRectLength : _arrowRectDepth;

    private bool _justRendered = false;
    private bool _hidden = true;
    
    protected override void OnInitialized()
    {
        MapModalService.OnUpdate += OnUpdate;
    }
    
    protected override void OnAfterRender(bool firstRender)
    {
        var justRendered = _justRendered;
        _justRendered = false;
        _hidden = false;

        if (justRendered && MapModalService.ModalType is not null) StateHasChanged();
    }

    public void Dispose()
    {
        MapModalService.OnUpdate -= OnUpdate;
    }

    private void OnUpdate()
    {
        var modalDimensionsAttribute = MapModalService.ModalType?.GetAttribute<MapModalDimensionsAttribute>();
        
        if (modalDimensionsAttribute is not null)
        {
            var width = modalDimensionsAttribute.Width;
            var height = modalDimensionsAttribute.Height;
            _arrowRectDepth = modalDimensionsAttribute.ArrowRectDepth;
            _arrowRectLength = modalDimensionsAttribute.ArrowRectLength;
            // Console.WriteLine($"MODAL type = {MapModalService.ModalType}, width = {width}, height = {height}");
            _dimensions.Set(width, height);
        }

        if (MapModalService.PrevModalType is null || 
            (MapModalService.PrevModalType != MapModalService.ModalType && MapModalService.TriggeredManually))
        {
            _hidden = true;
            _justRendered = true;
        }
        
        StateHasChanged();
    }
}
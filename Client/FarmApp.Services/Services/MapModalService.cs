using Bch.Modules.Maths.Models;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Attributes;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Extensions;

namespace FarmApp.Services.Services;

public class MapModalService : IMapModalService
{
    public event Action? OnUpdate;
    public MapModalType? ModalType { get; private set; }
    public MapModalType? PrevModalType { get; private set; }
    public bool TriggeredManually { get; private set; }
    public string? ModalId { get; private set; }
    public bool IsShown => ModalType != null;
    public Vec2 Position { get; } = new();
    public Vec2 RelativePoint { get; } = new();
    public int ArrowMode { get; private set; }

    public string Show(MapModalType? type, bool triggeredManually = true)
    {
        TriggeredManually = triggeredManually;
        PrevModalType = ModalType;
        ModalType = type;
        RecalculatePosition();
        OnUpdate?.Invoke();

        ModalId = Guid.NewGuid().ToString();
        return ModalId;
    }

    public string Show(MapModalType? type, float x, float y, bool triggeredManually = true)
    {
        TriggeredManually = triggeredManually;
        PrevModalType = ModalType;
        ModalType = type;
        RelativePoint.Set(x, y);
        RecalculatePosition();
        
        OnUpdate?.Invoke();
        
        ModalId = Guid.NewGuid().ToString();
        return ModalId;
    }

    private void RecalculatePosition()
    {
        var dimensionsAttribute = ModalType?.GetAttribute<MapModalDimensionsAttribute>();
        if (dimensionsAttribute is null) return;
        
        var modalWidth = dimensionsAttribute.Width;
        var modalHeight = dimensionsAttribute.Height;
        var arrowRectDepth = dimensionsAttribute.ArrowRectDepth;

        var modalHalfWidth = modalWidth * 0.5f;
        var modalHalfHeight = modalHeight * 0.5f;
            
        var cssWidth = ScreenOffsetProvider.ScreenWidth / ScreenOffsetProvider.Density;
        var cssHeight = ScreenOffsetProvider.ScreenHeight / ScreenOffsetProvider.Density;

        var screenHalfWidth = cssWidth * 0.5f;
        var screenHalfHeight = cssHeight * 0.5f;

        ArrowMode = RelativePoint.Y > screenHalfHeight ? 0 : 1;

        Position.X = RelativePoint.X - modalHalfWidth;
        Position.Y = RelativePoint.Y  > screenHalfHeight ? RelativePoint.Y - modalHeight - arrowRectDepth : RelativePoint.Y + arrowRectDepth;

        if (Position.X < 0)
        {
            Position.X = RelativePoint.X + arrowRectDepth;
            Position.Y = RelativePoint.Y - modalHalfHeight;
            ArrowMode = 2;
        }
        else if (Position.X + modalWidth > cssWidth)
        {
            Position.X = RelativePoint.X - modalWidth - arrowRectDepth;
            Position.Y = RelativePoint.Y - modalHalfHeight;
            ArrowMode = 3;
        }
    }
    
    public void Hide()
    {
        ModalId = null;
        ModalType = null;
        PrevModalType = null;
        OnUpdate?.Invoke();
    }
}
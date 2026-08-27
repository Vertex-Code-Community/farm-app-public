using Bch.Modules.Maths.Models;
using FarmApp.Shared.Enums;

namespace FarmApp.Services.Services.Interfaces;

public interface IMapModalService
{
    event Action? OnUpdate;
    MapModalType? ModalType { get; }
    MapModalType? PrevModalType { get; }
    bool TriggeredManually { get; }
    string? ModalId { get; }
    bool IsShown { get; }
    Vec2 Position { get; }
    Vec2 RelativePoint { get; }
    int ArrowMode { get; }
    
    string Show(MapModalType? type, bool triggeredManually = true);
    string Show(MapModalType? type, float x, float y, bool triggeredManually = true);
    void Hide();
}
using FarmApp.Shared.Math;

namespace FarmApp.Services.Services.Interfaces;

public interface ILocationService
{
    Task<bool> RequestPermissionAsync();
    Task<Vec2?> GetUserLocationAsync();
}
using FarmApp.Shared.Math;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.WebClient.Services;

public class BrowserLocationService : ILocationService
{
    public Task<bool> RequestPermissionAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Vec2?> GetUserLocationAsync()
    {
        throw new NotImplementedException();
    }
}
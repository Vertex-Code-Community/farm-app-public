using FarmApp.Services.Services.Interfaces;

namespace FarmApp.WebClient.Services;

public class BrowserAdService : IAdService
{
    public event Action? OnRewarded;
    public event Action? OnClosed;
    public Task<bool> LoadRewardAdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> ShowRewardAdAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsRewardEarnedAsync()
    {
        throw new NotImplementedException();
    }
}
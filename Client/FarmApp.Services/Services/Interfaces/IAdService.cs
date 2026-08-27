namespace FarmApp.Services.Services.Interfaces;

public interface IAdService
{
    event Action OnRewarded;
    event Action OnClosed;
    
    Task<bool> LoadRewardAdAsync();
    Task<bool> ShowRewardAdAsync();
    Task<bool> IsRewardEarnedAsync();
}
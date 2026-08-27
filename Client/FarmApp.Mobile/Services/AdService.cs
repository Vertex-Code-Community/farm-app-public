// using Plugin.MauiMTAdmob;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services;

public class AdService : IAdService, IDisposable
{
    public event Action? OnRewarded;
    public event Action? OnClosed;
    
    private TaskCompletionSource<bool>? _loadAdTaskCompletionSource;
    private TaskCompletionSource<bool>? _showAdTaskCompletionSource;
    private TaskCompletionSource<bool>? _rewardTaskCompletionSource;
    private const string REWARD_AD_BLOCK = "ca-app-pub-2026521099861719/4713610289";

    public AdService()
    {
        // CrossMauiMTAdmob.Current.OnRewardedLoaded += OnRewardedLoaded;
        // CrossMauiMTAdmob.Current.OnRewardedFailedToLoad += OnRewardedFailedToLoad;
        //
        // CrossMauiMTAdmob.Current.OnRewardedFailedToShow += OnRewardedFailedToShow;
        // CrossMauiMTAdmob.Current.OnRewardedOpened += OnRewardedOpened;
        //
        // CrossMauiMTAdmob.Current.OnRewardedClosed += OnRewardedClosed;
        //
        // CrossMauiMTAdmob.Current.OnUserEarnedReward += OnUserEarnedReward;
        // CrossMauiMTAdmob.Current.OnRewardedClicked += OnRewardedClicked;
        // CrossMauiMTAdmob.Current.OnRewardedImpression += OnRewardedImpression;
    }
    
    public void Dispose()
    {
        // CrossMauiMTAdmob.Current.OnRewardedLoaded -= OnRewardedLoaded;
        // CrossMauiMTAdmob.Current.OnRewardedFailedToLoad -= OnRewardedFailedToLoad;
        //
        // CrossMauiMTAdmob.Current.OnRewardedFailedToShow -= OnRewardedFailedToShow;
        // CrossMauiMTAdmob.Current.OnRewardedOpened -= OnRewardedOpened;
        //
        // CrossMauiMTAdmob.Current.OnRewardedClosed -= OnRewardedClosed;
        //
        // CrossMauiMTAdmob.Current.OnUserEarnedReward -= OnUserEarnedReward;
        // CrossMauiMTAdmob.Current.OnRewardedClicked -= OnRewardedClicked;
        // CrossMauiMTAdmob.Current.OnRewardedImpression -= OnRewardedImpression;
    }
    
    private void OnRewardedLoaded(object? sender, EventArgs e)
    {
        _loadAdTaskCompletionSource?.TrySetResult(true);
        Console.WriteLine("ADV OnRewardedLoaded");
    }
    
    private void OnRewardedFailedToLoad(object? sender, EventArgs e)
    {
        _loadAdTaskCompletionSource?.TrySetResult(false);
        Console.WriteLine("ADV OnRewardedFailedToLoad");
    }
    
    private void OnRewardedOpened(object? sender, EventArgs e)
    {
        _showAdTaskCompletionSource?.TrySetResult(true);
        Console.WriteLine("ADV OnRewardedOpened");
    }
    
    private void OnRewardedFailedToShow(object? sender, EventArgs e)
    {
        _showAdTaskCompletionSource?.TrySetResult(false);
        Console.WriteLine("ADV OnRewardedFailedToShow");
    }
    
    private void OnRewardedClosed(object? sender, EventArgs e)
    {
        _rewardTaskCompletionSource?.TrySetResult(false);
        Console.WriteLine("ADV OnRewardedClosed");
    }
    
    private void OnUserEarnedReward(object? sender, EventArgs e)
    {
        _rewardTaskCompletionSource?.TrySetResult(true);
        Console.WriteLine("ADV OnUserEarnedReward");
    }  
    
    private void OnRewardedClicked(object? sender, EventArgs e)
    {
        Console.WriteLine("ADV OnRewardedClicked");
    }
    
    private void OnRewardedImpression(object? sender, EventArgs e)
    {
        Console.WriteLine("ADV OnRewardedImpression");
    }

    public async Task<bool> LoadRewardAdAsync()
    {
        // 1. Loading in progress, wait
        if (_loadAdTaskCompletionSource is not null) return await _loadAdTaskCompletionSource.Task;

        // 2. Loaded before
        // if (CrossMauiMTAdmob.Current.IsRewardedLoaded())
        // {
        //     _loadAdTaskCompletionSource = null;
        //     return true;
        // } 
        
        // 3. Load new and wait
        _loadAdTaskCompletionSource = new TaskCompletionSource<bool>();
        Console.WriteLine($"ADV LoadRewarded");
        // CrossMauiMTAdmob.Current.LoadRewarded(REWARD_AD_BLOCK);
        var result = await _loadAdTaskCompletionSource.Task;
        _loadAdTaskCompletionSource = null;
        
        return result;
    }

    public async Task<bool> ShowRewardAdAsync()
    {
        // 1. Showing requested, wait
        if (_showAdTaskCompletionSource is not null) return await _showAdTaskCompletionSource.Task;
        
        // 2. Is not loaded
        // if (!CrossMauiMTAdmob.Current.IsRewardedLoaded()) return false;
        
        _showAdTaskCompletionSource = new TaskCompletionSource<bool>();

        // 3. Show and wait for callback
        Console.WriteLine($"ADV ShowRewarded");
        // CrossMauiMTAdmob.Current.ShowRewarded();

        var result = await _showAdTaskCompletionSource.Task;
        _showAdTaskCompletionSource = null;
        _rewardTaskCompletionSource = new TaskCompletionSource<bool>();

        return result;
    }

    public async Task<bool> IsRewardEarnedAsync()
    {
        if (_rewardTaskCompletionSource is null) return false;
        return await _rewardTaskCompletionSource.Task;
    }
}
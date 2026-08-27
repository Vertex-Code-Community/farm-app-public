using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services;

public class FirstTimeSetupService : IFirstTimeSetupService
{
    private const string Key = "FirstTimeSetupCompleted";

    public bool IsCompleted => Preferences.Get(Key, false);

    public void MarkCompleted() => Preferences.Set(Key, true);
}

using Android.Provider;
using FarmApp.Mobile.Services.Interfaces;

namespace FarmApp.Mobile.Services;

public class DeviceInfoAndroidService : IDeviceInfoService
{
    
    public Task<string> GetDeviceId()
    {
        return Task.FromResult(Settings.Secure.GetString(Platform.AppContext.ContentResolver, Settings.Secure.AndroidId) ?? string.Empty);
    }

    public Task<string> GetSystemVersion()
    {
        return Task.FromResult(DeviceInfo.Current.VersionString);
    }
}
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services
{
    public class MobilePlatformService : IMobilePlatformService
    {
        public bool IsIos => DeviceInfo.Platform == DevicePlatform.iOS;
    }
}

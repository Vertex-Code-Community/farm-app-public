using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services
{
    public class MauiPlatformContext : IPlatformContext
    {
        public bool isWeb => false;
    }
}

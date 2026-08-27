using FarmApp.Services.Services.Interfaces;

namespace FarmApp.WebClient.Services
{
    public class WebPlatformContext : IPlatformContext
    {
        public bool isWeb => true;
    }
}

using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Accounts;

namespace FarmApp.Services.Services
{
    public class AppleAuthService : IExternalAuthService
    {
        public ExternalAuthProvider ExternalAuthProvider => ExternalAuthProvider.Apple;

        public Task<ExternalAuthResult?> LoginAsync()
        {
            throw new NotImplementedException();
        }
    }
}

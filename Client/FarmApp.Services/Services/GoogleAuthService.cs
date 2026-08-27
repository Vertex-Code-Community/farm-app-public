using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Accounts;

namespace FarmApp.Services.Services
{
    public class GoogleAuthService : IExternalAuthService
    {
        public ExternalAuthProvider ExternalAuthProvider => ExternalAuthProvider.Google;

        public Task<ExternalAuthResult?> LoginAsync()
        {
            throw new NotImplementedException();
        }
    }
}

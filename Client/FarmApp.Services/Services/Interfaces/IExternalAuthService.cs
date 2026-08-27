using FarmApp.ViewModels.Accounts;

namespace FarmApp.Services.Services.Interfaces
{
    public interface IExternalAuthService
    {
        ExternalAuthProvider ExternalAuthProvider { get; }
        Task<ExternalAuthResult?> LoginAsync();
    }
}

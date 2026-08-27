using FarmApp.Models;
using FarmApp.ViewModels.Accounts;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IVerificationService
    {
        Task<bool> SendCodeAsync(string email, VerificationPurpose purpose, object? payload);
        ApiResponse VerifyCode(string email, string code, VerificationPurpose purpose);
        Task<ApiResponse> ResendCodeAsync(string email, VerificationPurpose purpose);
        object? GetPayload(string email, VerificationPurpose purpose);
        void Remove(string email, VerificationPurpose purpose);
    }
}

using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Verifications;

namespace FarmApp.Services.Services.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResult> SignInAsync(SignInRequestModel requestModel);
    Task<AuthResult> SignInExternal(ExternalAuthProvider authProvider);
    Task<AuthResult> SignUpAsync(SignUpRequestModel model);
    Task<AuthResult> ResendCodeAsync(VerificationResendRequestModel model);
    Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequestModel model);
    Task<AuthResult> ChangePasswordAsync(ChangePasswordRequestModel model);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordRequestModel model);
    Task<AuthResult<ResetTokenModelResponse>> ValidateResetCode(ResetCodeModel model);
    Task<AuthResult> ConfirmEmailAsync(ConfirmEmailRequestModel requestModel);
    void LogOut();
}
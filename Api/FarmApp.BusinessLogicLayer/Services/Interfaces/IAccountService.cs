using FarmApp.Models;
using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Verifications;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface IAccountService
{
    Task<ApiResponse> SignUpAsync(SignUpRequestModel model);
    Task<ApiResponse<TokenModel>> SignInAsync(SignInRequestModel requestModel);
    Task<ApiResponse<TokenModel>> ConfirmEmailAsync(ConfirmEmailRequestModel requestModel);
    Task<ApiResponse> ResendCodeAsync(VerificationResendRequestModel requestModel);
    Task<TokenModel?> UpdateTokensAsync(string refreshToken);
    ApiResponse<ResetTokenModelResponse> ValidateResetCode(ResetCodeModel model);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequestModel requestModel);
    Task<ApiResponse<TokenModel>> ResetPasswordAsync(ResetPasswordRequestModel model);
}

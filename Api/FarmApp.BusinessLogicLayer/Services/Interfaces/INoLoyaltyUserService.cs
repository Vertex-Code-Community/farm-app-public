using FarmApp.Models;
using FarmApp.Models.NoLoyalty;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface INoLoyaltyUserService
{
    Task<ApiResponse<NoLoyaltyTokensModel>> SignInAsync(NoLoyaltySignInModel model);
    Task<ApiResponse> SignUpAsync(NoLoyaltySignUpModel model);
    Task<NoLoyaltyTokensModel?> RefreshTokenAsync(NoLoyaltyRefreshRequestModel model);
    Task<bool> ConfirmEmailAsync(NoLoyaltyConfirmEmailRequest model);
    Task<ApiResponse> RequestPasswordResetAsync(NoLoyaltyRequestPasswordResetModel model);
    Task<ApiResponse> ConfirmPasswordResetAsync(NoLoyaltyConfirmPasswordResetRequest model);
}
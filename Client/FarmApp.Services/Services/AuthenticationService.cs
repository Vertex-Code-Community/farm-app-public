using FarmApp.Models;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Verifications;

namespace FarmApp.Services.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAppStoreService _appStoreService;
    private readonly IHttpService _httpService;
    private readonly AuthStateProvider _authStateProvider;
    private readonly IPlatformContext _platformContext;
    private readonly IEnumerable<IExternalAuthService> _externalAuthServices;

    public AuthenticationService(IAppStoreService appStoreService, IHttpService httpService,
        AuthStateProvider authStateProvider, IPlatformContext platformContext,
        IEnumerable<IExternalAuthService> externalAuthServices
        )
    {
        _appStoreService = appStoreService;
        _httpService = httpService;
        _authStateProvider = authStateProvider;
        _platformContext = platformContext;
        _externalAuthServices = externalAuthServices;
    }
    
    public async Task<AuthResult> SignInAsync(SignInRequestModel requestModel)
    {
        var apiResponse = await _httpService.PostAsync<ApiResponse<TokenModel>, SignInRequestModel>("api/account/sign-in", requestModel);

        if (apiResponse is null)
            return AuthResult.Fail("NETWORK_ERROR");

        if (apiResponse.Result == "Error")
            return AuthResult.Fail(apiResponse.Message ?? "LOGIN_FAILED");

        var tokenModel = apiResponse.Data;

        if (tokenModel is null || string.IsNullOrWhiteSpace(tokenModel.AccessToken))
        {
            _appStoreService.SetItem(Constants.JwtDetails.ACCESS_TOKEN, string.Empty);
            return AuthResult.Fail("INVALID_TOKEN_RESPONSE");
        }

        SaveTokens(tokenModel);
        await _authStateProvider.NotifyUserAuthentication();

        return AuthResult.Success();
    }

    public async Task<AuthResult> SignInExternal(ExternalAuthProvider authProvider)
    {
        var service = _externalAuthServices.First(x => x.ExternalAuthProvider == authProvider);

        var result = await service.LoginAsync();

        if (result == null)
            return AuthResult.Fail($"Failed to log in while accessing to {authProvider.ToString()} provider");

        var apiResponse = await _httpService.PostAsync<ApiResponse<TokenModel>, ExternalAuthResult>("api/account/external-sign-in", result);

        if (apiResponse is null)
            return AuthResult.Fail("NETWORK_ERROR");

        if (apiResponse.Result == "Error")
            return AuthResult.Fail(apiResponse.Message ?? "LOGIN_FAILED");

        var tokenModel = apiResponse.Data;

        if (tokenModel is null || string.IsNullOrWhiteSpace(tokenModel.AccessToken))
        {
            _appStoreService.SetItem(Constants.JwtDetails.ACCESS_TOKEN, string.Empty);
            return AuthResult.Fail("INVALID_TOKEN_RESPONSE");
        }
        SaveTokens(tokenModel);
        await _authStateProvider.NotifyUserAuthentication();

        return AuthResult.Success();
    }

    public async Task<AuthResult> SignUpAsync(SignUpRequestModel model)
    
    {
        var apiResponse = await _httpService.PostAsync<ApiResponse, SignUpRequestModel>("api/account/sign-up", model);
        if (apiResponse is null)
            return AuthResult.Fail("NETWORK_ERROR");

        if (apiResponse.Result == "Error")
            return AuthResult.Fail(apiResponse.Message ?? "SIGN_UP_FAILED");

        return AuthResult.Success();
    }

    public async Task<AuthResult> ConfirmEmailAsync(ConfirmEmailRequestModel requestModel)
    {
        var apiResponse = await _httpService.PostAsync<ApiResponse<TokenModel>, ConfirmEmailRequestModel>("api/account/confirm-email", requestModel);

        if (apiResponse is null)
            return AuthResult.Fail("NETWORK_ERROR");

        if (apiResponse.Result == "Error")
            return AuthResult.Fail(apiResponse.Message ?? "CONFIRMATION_EMAIL_FAILED");

        var tokenModel = apiResponse.Data;

        if (tokenModel is null || string.IsNullOrWhiteSpace(tokenModel.AccessToken))
        {
            _appStoreService.SetItem(Constants.JwtDetails.ACCESS_TOKEN, string.Empty);
            return AuthResult.Fail("INVALID_TOKEN_RESPONSE");
        }
        SaveTokens(tokenModel);
        await _authStateProvider.NotifyUserAuthentication();

        return AuthResult.Success();
    }

    public async Task<AuthResult> ResendCodeAsync(VerificationResendRequestModel model)
    {
        var apiReponse = await _httpService.PostAsync<ApiResponse, VerificationResendRequestModel>("api/account/resend-code",model);

        if (apiReponse is null)
            return AuthResult.Fail("NETWORK_ERROR");
        if (apiReponse.Result == "Error")
            return AuthResult.Fail(apiReponse.Message ?? "RESEND_CODE_ERROR");

        return AuthResult.Success();
    }

    public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequestModel model)
    {
        var result = await _httpService.PostAsync<ApiResponse, ForgotPasswordRequestModel>("api/account/forgot-password", model);

        if (result is null)
            return AuthResult.Fail("NETWORK_ERROR");
        if (result.Result == "Error")
            return AuthResult.Fail(result.Message ?? "CODE_IS_NOT_SENT");

        return AuthResult.Success();
    }
    public async Task<AuthResult> ChangePasswordAsync(ChangePasswordRequestModel model)
    {
        var result = await _httpService.PostAsync<ApiResponse, ChangePasswordRequestModel>("api/account/change-password", model);

        if (result is null)
            return AuthResult.Fail("NETWORK_ERROR");
        if (result.Result == "Error")
            return AuthResult.Fail(result.Message ?? "CHANGE_PASSWORD_FAILED");

        return AuthResult.Success();
    }
    public async Task<AuthResult<ResetTokenModelResponse>> ValidateResetCode(ResetCodeModel model)
    {
        var result = await _httpService.PostAsync<ApiResponse<ResetTokenModelResponse>, ResetCodeModel>("api/account/validate-reset-code", model);

        if (result is null)
            return AuthResult<ResetTokenModelResponse>.Fail("NETWORK_ERROR");

        if (result.Result == "Error")
            return AuthResult<ResetTokenModelResponse>.Fail(result.Message ?? "VALIDATE_CODE_FAILED");

        if (result.Data is null || string.IsNullOrEmpty(result.Data.ResetToken))
            return AuthResult<ResetTokenModelResponse>.Fail("VALIDATE_CODE_FAILED");


        return AuthResult<ResetTokenModelResponse>.Success(result.Data);
    }
    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequestModel model)
    {
        var result = await _httpService.PostAsync<ApiResponse<TokenModel>, ResetPasswordRequestModel>("api/account/reset-password", model);

        if (result is null)
            return AuthResult.Fail("NETWORK_ERROR");

        if (result.Result == "Error")
            return AuthResult.Fail(result.Message ?? "RESET_PASSWORD_FAILED");

        var tokenModel = result.Data;

        if (tokenModel is null || string.IsNullOrWhiteSpace(tokenModel.AccessToken))
        {
            _appStoreService.SetItem(Constants.JwtDetails.ACCESS_TOKEN, string.Empty);
            return AuthResult.Fail("INVALID_TOKEN_RESPONSE");
        }

        SaveTokens(tokenModel);
        await _authStateProvider.NotifyUserAuthentication();

        return AuthResult.Success();
    }
    public void LogOut()
    {
        _appStoreService.SetItem(Constants.JwtDetails.ACCESS_TOKEN, string.Empty);
        _authStateProvider.NotifyUserLogout();
    }
    private void SaveTokens(TokenModel tokenModel)
    {
        _appStoreService.SetItem(Constants.JwtDetails.ACCESS_TOKEN, tokenModel.AccessToken);
        if (!_platformContext.isWeb)
        {
            _appStoreService.SetItem(Constants.JwtDetails.REFRESH_TOKEN, tokenModel.RefreshToken);
        }
    }
}
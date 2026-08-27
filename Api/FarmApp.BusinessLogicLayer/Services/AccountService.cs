using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Models;
using FarmApp.Shared.Exceptions;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Users;
using FarmApp.ViewModels.Verifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static FarmApp.Shared.Constants.Constants;

namespace FarmApp.BusinessLogicLayer.Services;

public class AccountService : IAccountService
{
    private readonly JwtProvider _jwtProvider;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _memoryCache;
    private readonly IVerificationService _verificationService;

    public AccountService(
        JwtProvider jwtProvider,
        IUserService userService,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache memoryCache,
        IVerificationService verificationService)
    {
        _jwtProvider = jwtProvider;
        _userService = userService;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _memoryCache = memoryCache;
        _verificationService = verificationService;
    }

    public async Task<ApiResponse> SignUpAsync(SignUpRequestModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();

        if (!new EmailAddressAttribute().IsValid(email))
            return ApiResponses.Error(ErrorMessages.EMAIL_IS_INVALID);

        var userExists = await _userRepository.GetByEmailAsync(email);
        if (userExists != null)
            return ApiResponses.Error(ErrorMessages.USER_ALREADY_EXISTS);

        (string hash, string salt) = SecurityHelper.HashPassword(model.Password);

        var paylaod = new SignUpCacheModel
        {
            Email = email,
            PasswordHash = hash,
            Salt = salt,
        };

        var emailSendResult = await _verificationService.SendCodeAsync(email, VerificationPurpose.SignUp, paylaod);

        if (!emailSendResult)
            return ApiResponses.Error(ErrorMessages.CODE_IS_NOT_SENT);

        return ApiResponses.Ok();
    }

    public async Task<ApiResponse> ResendCodeAsync(VerificationResendRequestModel requestModel)
    {
        var email = requestModel.Email.Trim().ToLower();

        var result = await _verificationService.ResendCodeAsync(email, requestModel.VerificationPurpose);

        if (result.Result == "Error")
            return ApiResponses.Error(result.Message!);
        return ApiResponses.Ok();
    }

    public async Task<ApiResponse<TokenModel>> SignInAsync(SignInRequestModel requestModel)
    {
        if (!new EmailAddressAttribute().IsValid(requestModel.Email))
            return ApiResponses.Error<TokenModel>(ErrorMessages.EMAIL_OR_PASSWORD_IS_INCORRECT);

        var user = await _userRepository.GetByEmailAsync(requestModel.Email);

        if (user is null)
            return ApiResponses.Error<TokenModel>(ErrorMessages.USER_DOES_NOT_EXIST);

        if (!user.EmailConfirmed)
            return ApiResponses.Error<TokenModel>(ErrorMessages.EMAIL_IS_NOT_CONFIRMED);

        var isPasswordOk = SecurityHelper.VerifyPassword(
            requestModel.Password,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isPasswordOk)
            return ApiResponses.Error<TokenModel>(ErrorMessages.EMAIL_OR_PASSWORD_IS_INCORRECT);

        var claims = await _jwtProvider.GetUserClaimsAsync(user);

        var accessToken = _jwtProvider.GenerateAccessToken(claims);
        var refreshTokenModel = _jwtProvider.GenerateRefreshToken();

        var refreshToken = refreshTokenModel.RefreshToken;
        var refreshTokenHash = Hash(refreshTokenModel.RefreshToken);
        user.RefreshToken = refreshTokenHash;
        user.RefreshTokenLifeTime = refreshTokenModel.RefreshTokenLifetime;

        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Response.Cookies.Append(
                "refresh_token",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
        }

        await _userRepository.UpdateAsync(user);

        var result = new TokenModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
        return ApiResponses.Ok(result);
    }

    public async Task<ApiResponse<TokenModel>> ConfirmEmailAsync(ConfirmEmailRequestModel requestModel)
    {
        var email = requestModel.Email.Trim().ToLowerInvariant();

        if (!new EmailAddressAttribute().IsValid(email))
            throw new ServerException(ErrorMessages.EMAIL_IS_INVALID, HttpStatusCode.BadRequest);

        var existingUser = await _userRepository.GetByEmailAsync(email);

        if (existingUser != null)
            return ApiResponses.Error<TokenModel>(ErrorMessages.USER_ALREADY_EXISTS);


        var result = _verificationService.VerifyCode(email, requestModel.Code, VerificationPurpose.SignUp);

        if (result.Result == "Error")
            return ApiResponses.Error<TokenModel>(result.Message!);

        var payload = _verificationService.GetPayload(email, VerificationPurpose.SignUp);

        if (payload == null)
            return ApiResponses.Error<TokenModel>("Error");

        if (payload is not SignUpCacheModel userCacheModel)
            return ApiResponses.Error<TokenModel>(ErrorMessages.INVALID_TOKEN);

        _verificationService.Remove(email, VerificationPurpose.SignUp);

        var userEntity = await _userService.CreateAsync(new CreateUserModel
        {
            Email = email,
            PasswordHash = userCacheModel!.PasswordHash,
            Salt = userCacheModel.Salt,
        });

        userEntity.EmailConfirmed = true;

        var claims = await _jwtProvider.GetUserClaimsAsync(userEntity);

        var accessToken = _jwtProvider.GenerateAccessToken(claims);
        var refreshTokenModel = _jwtProvider.GenerateRefreshToken();

        var refreshToken = refreshTokenModel.RefreshToken;
        var refreshTokenHash = Hash(refreshTokenModel.RefreshToken);

        userEntity.RefreshToken = refreshTokenHash;
        userEntity.RefreshTokenLifeTime = refreshTokenModel.RefreshTokenLifetime;
        await _userRepository.UpdateAsync(userEntity);


        var tokenModel = new TokenModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return ApiResponses.Ok(tokenModel);
    }


    public async Task<TokenModel?> UpdateTokensAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"] ?? "";
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var hashedToken = Hash(refreshToken);

        var user = await _userRepository.GetByRefreshTokenAsync(hashedToken);

        if (user == null || user.RefreshTokenLifeTime < DateTime.UtcNow)
            return null;

        var claims = await _jwtProvider.GetUserClaimsAsync(user);
        var accessToken =  _jwtProvider.GenerateAccessToken(claims);

        var newRefreshTokenModel = _jwtProvider.GenerateRefreshToken();
        var newRefreshToken = newRefreshTokenModel.RefreshToken;
        var newRefreshTokenHash = Hash(newRefreshTokenModel.RefreshToken);
        user.RefreshToken = newRefreshTokenHash;
        user.RefreshTokenLifeTime = newRefreshTokenModel.RefreshTokenLifetime;

        await _userRepository.UpdateAsync(user);

        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Cookies.ContainsKey("refresh_token") == true)
        {
            context.Response.Cookies.Append(
                "refresh_token",
                newRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
        }

        return new TokenModel
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
        };
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequestModel model)
    {

        var email = model.Email.Trim().ToLowerInvariant();

        if (!new EmailAddressAttribute().IsValid(email))
            return ApiResponses.Ok();

        var userEntity = await _userRepository.GetByEmailAsync(email);

        if (userEntity is null || !userEntity.EmailConfirmed)
            return ApiResponses.Ok();


        var emailSendResult = await _verificationService.SendCodeAsync(email, VerificationPurpose.ResetPassword, null);

        if (!emailSendResult)
            return ApiResponses.Error(ErrorMessages.CODE_IS_NOT_SENT);

        return ApiResponses.Ok();

    }
    public ApiResponse<ResetTokenModelResponse> ValidateResetCode(ResetCodeModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();

        var result = _verificationService.VerifyCode(email, model.Code, VerificationPurpose.ResetPassword);

        if (result.Result == "Error")
            return ApiResponses.Error<ResetTokenModelResponse>(result.Message!);

        var resetToken = GenerateSecureToken();

        _memoryCache.Set($"reset_token_{resetToken}", email, TimeSpan.FromMinutes(10));

        var response = new ResetTokenModelResponse
        {
            ResetToken = resetToken
        };

        return ApiResponses.Ok(response);
    }

    public async Task<ApiResponse<TokenModel>> ResetPasswordAsync(ResetPasswordRequestModel model)
    {
        var resetTokenKey = $"reset_token_{model.ResetToken}";

        var email = model.Email.Trim().ToLowerInvariant();

        if (!new EmailAddressAttribute().IsValid(email))
            return ApiResponses.Error<TokenModel>(ErrorMessages.EMAIL_IS_INVALID);

        if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
            return ApiResponses.Error<TokenModel>(ErrorMessages.PASSWORD_IS_INCORRECT);

        if (!model.Password.Equals(model.ConfirmPassword))
            return ApiResponses.Error<TokenModel>(ErrorMessages.PASSWORDS_DONT_MATCH);

        var cachedEmail = _memoryCache.Get<string>(resetTokenKey);

        if (cachedEmail is null)
            return ApiResponses.Error<TokenModel>(ErrorMessages.INVALID_TOKEN);

        if (!string.Equals(cachedEmail,email, StringComparison.Ordinal))
            return ApiResponses.Error<TokenModel>(ErrorMessages.INVALID_TOKEN);

        var userEntity = await _userRepository.GetByEmailAsync(email);

        if (userEntity is null)
            return ApiResponses.Error<TokenModel>(ErrorMessages.INVALID_TOKEN);

        _memoryCache.Remove(resetTokenKey);

        var newHash = SecurityHelper.HashPassword(model.Password);

        userEntity.PasswordSalt = newHash.Salt;
        userEntity.PasswordHash = newHash.Hash;

        var claims = await _jwtProvider.GetUserClaimsAsync(userEntity);

        var accessToken = _jwtProvider.GenerateAccessToken(claims);
        var refreshTokenModel = _jwtProvider.GenerateRefreshToken();

        var refreshToken = Hash(refreshTokenModel.RefreshToken);
        userEntity.RefreshToken = refreshToken;

        userEntity.RefreshTokenLifeTime = refreshTokenModel.RefreshTokenLifetime;
        await _userRepository.UpdateAsync(userEntity);

        return ApiResponses.Ok(new TokenModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenModel.RefreshToken,
        });
    }
    private static string Hash(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hashBytes);
    }
    private static string GenerateSecureToken(int size = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToHexString(bytes);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FarmApp.Entities.Entity;
using FarmApp.Models;
using FarmApp.Models.Email;
using FarmApp.Models.NoLoyalty;
using FarmApp.Shared.Helpers;

namespace FarmApp.BusinessLogicLayer.Services.NoLoyaltyUser;

public partial class NoLoyaltyUserService
{
    private readonly DateTime _refreshTokenExpirationTime = DateTime.UtcNow.AddDays(14);
    public async Task<ApiResponse<NoLoyaltyTokensModel>> SignInAsync(NoLoyaltySignInModel model)
    {
        // TODO: consider better validation
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return ApiResponses.Error<NoLoyaltyTokensModel>("Email cannot be empty!");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            return ApiResponses.Error<NoLoyaltyTokensModel>("Password cannot be empty!");
        }
        
        var user = await _userRepository.GetByEmailAsync(model.Email);
        if (user is null)
        {
            return ApiResponses.Error<NoLoyaltyTokensModel>("User not found!");
        }

        if (!user.EmailConfirmed)
        {
            return ApiResponses.Error<NoLoyaltyTokensModel>("User not confirmed!");
        }

        if (!SecurityHelper.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
        {
            return ApiResponses.Error<NoLoyaltyTokensModel>("Wrong login or password!");
        }


        // Unset claims throw an exception
        // var tokens = await GenerateAndSaveTokensAsync(user);

        var claims = await _jwtProvider.GetUserClaimsAsync(user);

        var accessToken = _jwtProvider.GenerateAccessToken(claims);
        var refreshTokenModel = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = refreshTokenModel.RefreshToken;
        user.RefreshTokenLifeTime = refreshTokenModel.RefreshTokenLifetime;

        await _userRepository.UpdateAsync(user);

        var noLoyaltyTokensModel = new NoLoyaltyTokensModel()
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenModel.RefreshToken,
        };

        return ApiResponses.Ok(noLoyaltyTokensModel);
    }

    public async Task<NoLoyaltyTokensModel?> RefreshTokenAsync(NoLoyaltyRefreshRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.RefreshToken)) return null;

        var user = await _userRepository.GetByRefreshTokenAsync(model.RefreshToken);
        if (user is null) return null;

        return await GenerateAndSaveTokensAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(NoLoyaltyConfirmEmailRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Token)) return false;

        var user = await _userRepository.GetByEmailConfirmTokenAsync(model.Token);
        if (user is null) return false;

        if (user.EmailConfirmed) return true;

        if (string.IsNullOrEmpty(user.EmailConfirmToken)) return false;
        if (!string.Equals(user.EmailConfirmToken, model.Token, StringComparison.OrdinalIgnoreCase)) return false;

        user.EmailConfirmed = true;
        await _userRepository.UpdateAsync(user);

        return true;
    }

    public async Task<ApiResponse> RequestPasswordResetAsync(NoLoyaltyRequestPasswordResetModel model)
    {
        throw new NotImplementedException();
        
        if (string.IsNullOrWhiteSpace(model.Email))
            return new ApiResponse { Result = "Error", Message = "Email cannot be empty!" };

        var user = await _userRepository.GetByEmailAsync(model.Email);

        var resetToken = Guid.NewGuid().ToString("N");
        if (user is not null)
        {
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);
            await _userRepository.UpdateAsync(user);
        }

        var resetUrlPath = "reset-password";
        // var callbackUrl = new Uri($"{webAppUrl}{resetUrlPath}/{resetToken}");
        //
        // await _emailService.SendEmailAsync(new SendEmailRequest
        // {
        //     EmailTo = model.Email,
        //     EmailSubject = "Reset your password",
        //     EmailBody = $"To reset your password click <a href=\"{callbackUrl}\">this link</a>. If you did not request a password reset, please ignore this email."
        // });

        return new ApiResponse { Result = "Ok" };
    }

    public async Task<ApiResponse> ConfirmPasswordResetAsync(NoLoyaltyConfirmPasswordResetRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Token))
            return new ApiResponse { Result = "Error", Message = "Token is required" };
        if (string.IsNullOrWhiteSpace(model.NewPassword))
            return new ApiResponse { Result = "Error", Message = "Password cannot be empty!" };

        var user = await _userRepository.GetByPasswordResetTokenAsync(model.Token);
        if (user is null)
            return new ApiResponse { Result = "Error", Message = "Invalid or expired token" };

        var password = SecurityHelper.HashPassword(model.NewPassword);
        user.PasswordHash = password.Hash;
        user.PasswordSalt = password.Salt;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiration = null;
        await _userRepository.UpdateAsync(user);

        return new ApiResponse { Result = "Ok" };
    }

    private async Task<NoLoyaltyTokensModel> GenerateAndSaveTokensAsync(UserEntity user)
    {
        var issuedAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.FirstName),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Iat, issuedAt, ClaimValueTypes.Integer64),
        };

        var accessToken = _jwtProvider.GenerateAccessToken(claims);
        var refreshTokenModel = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = refreshTokenModel.RefreshToken;
        user.RefreshTokenLifeTime = refreshTokenModel.RefreshTokenLifetime;
        await _userRepository.UpdateAsync(user);

        var tokens = new NoLoyaltyTokensModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenModel.RefreshToken,
        };

        return tokens;
    }

    public async Task<ApiResponse> SignUpAsync(NoLoyaltySignUpModel model)
    {
        // TODO: consider better validation
        if (string.IsNullOrWhiteSpace(model.Email)) return new ApiResponse { Result = "Error", Message = "Email cannot be empty!" };
        if (string.IsNullOrWhiteSpace(model.Password)) return new ApiResponse { Result = "Error", Message = "Password cannot be empty!" };

        var existingUser = await _userRepository.GetByEmailAsync(model.Email);
        if (existingUser is not null) return new ApiResponse { Result = "Error", Message = "User already exist!" };

        var password = SecurityHelper.HashPassword(model.Password);

        var emailConfirmToken = Guid.NewGuid().ToString("N");

        var user = new UserEntity
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Address = model.Address,
            City = model.City,
            State = model.State,
            ZipCode = model.ZipCode,
            Email = model.Email,
            EmailConfirmToken = emailConfirmToken,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
        };

        await _userRepository.CreateAsync(user);
        await _userRepository.AddDefaultNotificationPreferencesAsync(user.Id);

        var webAppUrl = "";
        var confirmEmailUrl = "confirm-email";
        var callbackUrl = new Uri($"{webAppUrl}{confirmEmailUrl}/{emailConfirmToken}");

        var isSent = await _emailService.SendEmailAsync(new SendEmailRequest
        {
            EmailTo = model.Email,
            EmailSubject = "Confirm your account",
            EmailBody = $"Please confirm your account by clicking <a href=\"{callbackUrl}\">this link</a>."
        });

        if (!isSent)
        {
            await _userRepository.DeleteAsync(user);
            return new ApiResponse { Result = "Error", Message = "Problem with registering you in system, try later please!" };
        }
        
        return new ApiResponse { Result = "Ok" };
    }
}
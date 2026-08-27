using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmApp.ViewModels.Options;
using Org.BouncyCastle.Utilities;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using FarmApp.ViewModels.Accounts;

namespace FarmApp.BusinessLogicLayer.Providers;

public class JwtProvider
{
    private readonly IOptions<JwtConnectionOptions> _connectionOptions;

    public JwtProvider(IOptions<JwtConnectionOptions> connectionOptions)
    {
        _connectionOptions = connectionOptions;
    }

    public Task<List<Claim>> GetUserClaimsAsync(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimsIdentity.DefaultNameClaimType, user.Email!),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimsIdentity.DefaultRoleClaimType, user.Role.ToString()),
            new(ClaimTypes.Role, "Client") 
        };

        return Task.FromResult(claims);
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var token = GenerateToken(claims, _connectionOptions.Value.LifetimeAccessToken);
        return token;
    }
    
    public RefreshTokenResultModel GenerateRefreshToken(int size = 64)
    {
        var randomBytes = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = WebEncoders.Base64UrlEncode(randomBytes);
        var expiresAt = DateTime.Now.AddMinutes(_connectionOptions.Value.LifetimeRefreshToken);

        return new RefreshTokenResultModel
        {
            RefreshToken = refreshToken,
            RefreshTokenLifetime = expiresAt
        };
    }
    
    private string GenerateToken(IEnumerable<Claim> claims, int lifetime)
    {
        var token = new JwtSecurityToken(
           issuer: _connectionOptions.Value.Issuer,
           audience: _connectionOptions.Value.Audience,
           claims: claims,
           notBefore: DateTime.Now,
           expires: DateTime.Now.AddMinutes(lifetime),
           signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );

        var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);
        return generatedToken;
    }

    private SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        var symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_connectionOptions.Value.SecretKey));
        return symmetricKey;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSymmetricSecurityKey(),
            ValidateLifetime = false
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        catch
        {
            throw new Exception("Please input correct data");
        }
    }
}
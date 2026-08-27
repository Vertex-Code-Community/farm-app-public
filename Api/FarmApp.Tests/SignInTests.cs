using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.BusinessLogicLayer.Services.NoLoyaltyUser;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Models.NoLoyalty;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Options;
using Microsoft.Extensions.Options;
using Moq;

namespace FarmApp.Tests;

[TestFixture]
public class SignInTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IEmailService> _emailServiceMock;
    private JwtProvider _jwtProvider;
    private NoLoyaltyUserService _service;

    private const string TestEmail = "test@farm.com";
    private const string TestPassword = "Password123!";
    private const string JwtSecret = "SuperSecretKeyForTestingPurposes_AtLeast32Bytes!";

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();

        var jwtOptions = Options.Create(new JwtConnectionOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = JwtSecret,
            LifetimeAccessToken = 60,
            LifetimeRefreshToken = 1440,
            LengthRefreshToken = 64
        });

        _jwtProvider = new JwtProvider(jwtOptions);
        _service = new NoLoyaltyUserService(_userRepositoryMock.Object, _jwtProvider, _emailServiceMock.Object);
    }

    [Test]
    public async Task SignIn_ValidCredentials_ReturnsTokenWithCorrectRoleClaims()
    {
        // Arrange
        var password = SecurityHelper.HashPassword(TestPassword);

        var user = new UserEntity
        {
            Id = "user-123",
            Email = TestEmail,
            EmailConfirmed = true,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            Role = UserRole.Admin,
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(TestEmail))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<UserEntity>()))
            .Returns(Task.CompletedTask);

        var model = new NoLoyaltySignInModel { Email = TestEmail, Password = TestPassword };

        // Act
        var result = await _service.SignInAsync(model);

        // Assert — login succeeded
        Assert.That(result.Result, Is.EqualTo("Ok"));
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.AccessToken, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Data.RefreshToken, Is.Not.Null.And.Not.Empty);

        // Assert — decode JWT and verify claims/permissions
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Data.AccessToken);

        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.That(subClaim?.Value, Is.EqualTo("user-123"), "Token must contain correct user id");

        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        Assert.That(emailClaim?.Value, Is.EqualTo(TestEmail), "Token must contain correct email");

        // In JWT, DefaultRoleClaimType and ClaimTypes.Role both get mapped to
        // the "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" type
        var roleClaims = jwt.Claims
            .Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType
                        || c.Type == ClaimTypes.Role
                        || c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        Assert.That(roleClaims, Does.Contain("Admin"), "Token must contain Admin role");
        Assert.That(roleClaims, Does.Contain("Client"), "Token must contain Client role");

        // Assert — refresh token was saved to the user entity
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<UserEntity>(u =>
            u.RefreshToken != null && u.RefreshTokenLifeTime != null)), Times.Once);
    }

    [Test]
    public async Task SignIn_WrongPassword_ReturnsError()
    {
        // Arrange
        var password = SecurityHelper.HashPassword(TestPassword);

        var user = new UserEntity
        {
            Id = "user-123",
            Email = TestEmail,
            EmailConfirmed = true,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            Role = UserRole.User
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(TestEmail))
            .ReturnsAsync(user);

        var model = new NoLoyaltySignInModel { Email = TestEmail, Password = "WrongPassword!" };

        // Act
        var result = await _service.SignInAsync(model);

        // Assert
        Assert.That(result.Result, Is.EqualTo("Error"));
        Assert.That(result.Message, Is.EqualTo("Wrong login or password!"));
        Assert.That(result.Data, Is.Null);
    }

    [Test]
    public async Task SignIn_UnconfirmedEmail_ReturnsError()
    {
        // Arrange
        var password = SecurityHelper.HashPassword(TestPassword);

        var user = new UserEntity
        {
            Id = "user-123",
            Email = TestEmail,
            EmailConfirmed = false,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            Role = UserRole.User
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(TestEmail))
            .ReturnsAsync(user);

        var model = new NoLoyaltySignInModel { Email = TestEmail, Password = TestPassword };

        // Act
        var result = await _service.SignInAsync(model);

        // Assert
        Assert.That(result.Result, Is.EqualTo("Error"));
        Assert.That(result.Message, Is.EqualTo("User not confirmed!"));
    }

    [Test]
    public async Task SignIn_NonExistentUser_ReturnsError()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(TestEmail))
            .ReturnsAsync((UserEntity?)null);

        var model = new NoLoyaltySignInModel { Email = TestEmail, Password = TestPassword };

        // Act
        var result = await _service.SignInAsync(model);

        // Assert
        Assert.That(result.Result, Is.EqualTo("Error"));
        Assert.That(result.Message, Is.EqualTo("User not found!"));
    }
}

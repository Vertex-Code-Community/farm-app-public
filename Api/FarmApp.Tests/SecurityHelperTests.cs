using FarmApp.Shared.Helpers;

namespace FarmApp.Tests;

[TestFixture]
public class SecurityHelperTests
{
    [Test]
    public void HashPassword_ReturnsNonEmptyHashAndSalt()
    {
        // Arrange
        var password = "MySecret123!";

        // Act
        var result = SecurityHelper.HashPassword(password);

        // Assert
        Assert.That(result.Hash, Is.Not.Null.And.Not.Empty, "Hash should not be empty");
        Assert.That(result.Salt, Is.Not.Null.And.Not.Empty, "Salt should not be empty");
    }

    [Test]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecret123!";
        var hashed = SecurityHelper.HashPassword(password);

        // Act
        var isPasswordOk = SecurityHelper.VerifyPassword(password, hashed.Hash, hashed.Salt);
        // Assert
        Assert.That(isPasswordOk, Is.True, "Password is correct");
    }

    [Test]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var password = "MySecret123!";
        var wrongPassword = "WrongPassword!";
        var hashed = SecurityHelper.HashPassword(password);

        // Act
        var isPasswordOk = SecurityHelper.VerifyPassword(wrongPassword, hashed.Hash, hashed.Salt);
        // Assert
        Assert.That(isPasswordOk, Is.False, "Password is incorrect");
    }

    [Test]
    public void HashPassword_SamePassword_ProducesDifferentHashes()
    {
        // Arrange
        var password = "MySecret123!";

        // Act
        var hashed1 = SecurityHelper.HashPassword(password);
        var hashed2 = SecurityHelper.HashPassword(password);

        // Assert
        Assert.That(hashed2.Salt == hashed1.Salt, Is.False, "Hashing is correct");
    }
}

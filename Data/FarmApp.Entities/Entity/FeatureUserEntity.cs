using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class FeatureUserEntity : IBaseEntity<long>
{
    public long Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string EmailConfirmToken { get; set; }

    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiration { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenLifeTime { get; set; }

    public DateTime Created { get; set; }
}
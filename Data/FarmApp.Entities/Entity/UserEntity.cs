using FarmApp.Entities.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmApp.Entities.Entity;

[Table("users")]

public class UserEntity : IBaseEntity<string>
{
    public string Id { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }

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
    
    public UserRole Role { get; set; }
    public string? EmailVerificationCode { get; set; }
    public DateTime? VerificationSentDate { get; set; }
    public int IconNumber { get; set; }

    public double? SelectedLocationLatitude { get; set; }
    public double? SelectedLocationLongitude { get; set; }

    public UserNotificationPreferencesEntity? NotificationPreferences { get; set; }

    public List<PropertyEntity> Properties { get; set; } = new();
    public List<CustomSteadEntity> CustomSteads { get; set; } = new();
}

public enum UserRole
{
    User,
    Admin,
}

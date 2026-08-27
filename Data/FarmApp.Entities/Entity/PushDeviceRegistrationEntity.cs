using System.ComponentModel.DataAnnotations.Schema;

namespace FarmApp.Entities.Entity;

/// <summary>
/// Registered mobile device for push (tokens and tags for direct FCM delivery).
/// </summary>
[Table("push_device_registrations")]
public class PushDeviceRegistrationEntity
{
    /// <summary>Stable device id from the client (AndroidId / iOS identifierForVendor).</summary>
    public string DeviceId { get; set; } = string.Empty;

    public string DeviceToken { get; set; } = string.Empty;

    /// <summary>Original client platform string (e.g. fcmv1, apns).</summary>
    public string Platform { get; set; } = string.Empty;

    public string? UserId { get; set; }

    /// <summary>JSON array of tag strings (same semantics as NH installation tags).</summary>
    public string TagsJson { get; set; } = "[]";
}

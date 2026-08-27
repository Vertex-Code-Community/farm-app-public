namespace FarmApp.Models.PushNotification;

/// <summary>
/// Batch delivery summary (JSON shape compatible with legacy admin UI expectations).
/// </summary>
public sealed class NotificationDeliveryOutcome
{
    public long Success { get; set; }

    public long Failure { get; set; }

    public List<NotificationDeliveryRegistrationResult> Results { get; set; } = [];
}

public sealed class NotificationDeliveryRegistrationResult
{
    public string ApplicationPlatform { get; set; } = string.Empty;

    public string PnsHandle { get; set; } = string.Empty;

    public string RegistrationId { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;
}

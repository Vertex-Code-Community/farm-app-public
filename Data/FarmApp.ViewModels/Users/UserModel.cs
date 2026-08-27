using FarmApp.Shared.Enums;

namespace FarmApp.ViewModels.Users;

public class UserModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public int IconNumber { get; set; }
    public string? FondyOrderId { get; set; }
    public string? FondyLastRecurringPaymentStatus { get; set; }
    public DateTime? FondyLastRecurringPaymentTime { get; set; }
    public DateTime RegistrationTime { get; set; }

    public double? SelectedLocationLatitude { get; set; }
    public double? SelectedLocationLongitude { get; set; }

    public bool NotificationsDisabled { get; set; }

    public bool SystemNotificationsEnabled { get; set; } = true;
    public bool WeatherAlertsEnabled { get; set; } = true;
    public bool ActivityRemindersEnabled { get; set; } = true;
    public bool InAppNotificationsOnly { get; set; }
}

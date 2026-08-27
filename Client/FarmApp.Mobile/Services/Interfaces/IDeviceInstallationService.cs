using FarmApp.Models.PushNotification;

namespace FarmApp.Mobile.Services.Interfaces;

public interface IDeviceInstallationService
{
    TaskCompletionSource<string?> DeviceTokenTcs { get; set; }
    bool NotificationsSupported { get; }
    string? GetDeviceId();
    Task<DeviceInstallation?> GetDeviceInstallationAsync(params List<string> tags);
}
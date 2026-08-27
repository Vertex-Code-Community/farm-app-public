namespace FarmApp.Mobile.Services.Interfaces;

public interface IPushNotificationPermissionService
{
    Task<bool> RequestAndRegisterAsync();
    Task<PermissionStatus> CheckStatusAsync();
}
namespace FarmApp.Mobile.Services.Interfaces;

public interface IDeviceInfoService
{
    Task<string> GetDeviceId();
    Task<string> GetSystemVersion();
}
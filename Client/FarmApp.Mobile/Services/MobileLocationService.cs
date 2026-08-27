using FarmApp.Shared.Math;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Mobile.Services;

public class MobileLocationService : ILocationService
{
    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        if (status == PermissionStatus.Denied)
        {
            var result = await Application.Current.MainPage.DisplayAlert("Permission required", 
                "Location is needed to get weather for your area or your location on map",
                "Open settings","Cancel");

            if (result)
                AppInfo.ShowSettingsUI();
        }

        return status == PermissionStatus.Granted;
    }

    public async Task<Vec2?> GetUserLocationAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return null;

            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));

                location = await Geolocation.GetLocationAsync(request);
            }
            if (location == null)
                return null;
        
            if (location != null)
            {
                return new Vec2
                {
                    X = location.Longitude,
                    Y = location.Latitude
                };
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
        }
        catch (PermissionException pEx)
        {
        }
        catch (Exception ex)
        {
            return null;
        }

        return null;
    }
}
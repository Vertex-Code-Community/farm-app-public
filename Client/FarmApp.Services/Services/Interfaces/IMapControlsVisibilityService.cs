namespace FarmApp.Services.Services.Interfaces;

public enum MapControlButton
{
    Create,
    PropertyCreate,
    FlyToCountry,
    Compass,
    Location
}

public interface IMapControlsVisibilityService
{
    bool IsVisible(MapControlButton button);
    void SetVisibility(MapControlButton button, bool visible);

    event Action? Changed;
}

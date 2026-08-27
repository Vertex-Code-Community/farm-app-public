using MaplibreMaui.Services;

namespace FarmApp.Mobile.Services.Interfaces;

public interface IMapService
{
    IMaplibreMapService? MaplibreMapService { get; set; }
    void OnStyleLoaded();
    void OnBlazorServicesLoaded();
    bool OnDown(float x, float y);
    bool OnUp(float x, float y);
    bool OnClick(float x, float y);
    bool OnMapMove(float x, float y);
    void OnMapRotate();
}
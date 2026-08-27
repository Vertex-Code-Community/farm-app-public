using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Services;

public class MapCallbackService : IMapCallbackService, IDisposable
{
    private readonly IMapModalService _mapModalService;
    
    public MapCallbackService(IMapModalService mapModalService)
    {
        _mapModalService = mapModalService;
        
        IMapCallbackService.OnMove += OnMapMove;
        IMapCallbackService.OnClickOutsideOfStead += ClickedOutsideOfStead;
        //IMapCallbackService.OnClickOutsideOfCustomStead += ClickedOutsideOfStead;
    }
    
    public void Dispose()
    {
        IMapCallbackService.OnMove -= OnMapMove;
        IMapCallbackService.OnClickOutsideOfStead -= ClickedOutsideOfStead;
        //IMapCallbackService.OnClickOutsideOfCustomStead -= ClickedOutsideOfStead;
    }
    
    private void OnMapMove(float dx, float dy)
    {
        if (_mapModalService.IsShown)
            _mapModalService.Hide();
    }

    private void ClickedOutsideOfStead()
    {
        if (_mapModalService.IsShown)
            _mapModalService.Hide();
    }
}
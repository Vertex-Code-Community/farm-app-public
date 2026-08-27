using FarmApp.AdminComponents.Services.Interfaces;

namespace FarmApp.AdminComponents.Services;

public class HeaderControlsService : IHeaderControlsService
{
    private bool _showDate = false;

    public event Action? OnChanged;

    public bool ShowDate
    {
        get => _showDate;
        set
        {
            if (_showDate == value) return;
            _showDate = value;
            OnChanged?.Invoke();
        }
    }
}

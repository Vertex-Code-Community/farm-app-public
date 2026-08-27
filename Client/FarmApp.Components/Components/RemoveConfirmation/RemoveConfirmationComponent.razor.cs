using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.RemoveConfirmation;

public partial class RemoveConfirmationComponent
{
    [Parameter] public string Question { get; set; } = string.Empty;
    [Parameter] public EventCallback OnConfirm { get; set; }
    
    [Parameter] public EventCallback<bool> IsOpenedChanged { get; set; }
    [Parameter] public bool IsOpened
    {
        get => _isOpened;
        set
        {
            if (_isOpened == value) return;
            _isOpened = value;
            IsOpenedChanged.InvokeAsync(value);
        }
    }

    private bool _isOpened = false;
}
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.InfoComponent;

public partial class InfoModalComponent
{
    [Parameter] public EventCallback<bool> OpenedChanged { get; set; }
    [Parameter] public required string? Title { get; set; }
    [Parameter] public required string Info { get; set; } = "Info";

    [Parameter]
    public required bool Opened
    {
        get => _opened;
        set
        {
            if (_opened == value) return;
            _opened = value;
            OpenedChanged.InvokeAsync(value);
        }
    }

    private bool _opened;
}

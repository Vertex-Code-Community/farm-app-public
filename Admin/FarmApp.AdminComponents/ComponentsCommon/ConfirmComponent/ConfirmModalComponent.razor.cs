using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.ConfirmComponent;

public partial class ConfirmModalComponent
{
    [Parameter] public required bool Result { get; set; }
    [Parameter] public EventCallback<bool> ResultChanged { get; set; }
    [Parameter] public EventCallback<bool> OpenedChanged { get; set; }
    [Parameter] public required string? Title { get; set; }
    [Parameter] public required string? Message { get; set; }

    [Parameter] public string Width { get; set; } = "25%";

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

    private async Task OnConfirmAsync(bool result)
    {
        Opened = false;
        StateHasChanged();

        Result = result;
        await ResultChanged.InvokeAsync(result);
    }
}

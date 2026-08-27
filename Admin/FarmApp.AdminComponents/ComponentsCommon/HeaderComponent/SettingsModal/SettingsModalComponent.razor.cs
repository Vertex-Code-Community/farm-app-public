using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.HeaderComponent.SettingsModal;

public partial class SettingsModalComponent
{
    [Parameter] public bool Show { get; set; }
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }
    [Parameter] public EventCallback<DateTime?> OnDateChange { get; set; }
    [Inject] public required IHeaderControlsService HeaderControlsService { get; set; }

    private async Task OnDateChangeAsync(DateTime? date)
    {
        await OnDateChange.InvokeAsync(date);
    }
}

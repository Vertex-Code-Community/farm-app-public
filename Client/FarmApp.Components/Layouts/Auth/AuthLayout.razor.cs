using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Layouts.Auth;

public partial class AuthLayout
{
    [Inject] public required IJSRuntime JSRuntime { get; set; }

    private IJSObjectReference? _module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        if (firstRender)
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/FarmApp.Components/Layouts/Auth/AuthLayout.razor.js");
            await _module.InvokeVoidAsync("initStaggeredReveal", "auth-content");
            await _module.InvokeVoidAsync("initAutoHeight");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}

using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;
namespace FarmApp.AdminComponents.ComponentsCommon.GlobalLoader;

public partial class GlobalLoaderComponent : IDisposable
{
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }

    protected override void OnInitialized()
    {
        GlobalLoaderService.OnLoaderSwitch += StateHasChanged;
    }

    public void Dispose()
    {
        GlobalLoaderService.OnLoaderSwitch -= StateHasChanged;
    }
}

using Microsoft.AspNetCore.Components;
using FarmApp.Components.ViewModels.Tabs;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Components.Layouts.TabLayout.Tab;

public partial class TabComponent
{
    [Inject] public INavigationService NavigationService { get; set; }
    [Parameter] public TabConfigModel Config { get; set; }
    [Parameter] public bool Active { get; set; }

    private void OnTabClicked()
    {
        NavigationService.NavigateTo(Config.Reference);
    }
}

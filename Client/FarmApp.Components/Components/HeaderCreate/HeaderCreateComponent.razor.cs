using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.HeaderCreate
{
    public partial class HeaderCreateComponent
    {
        [Inject] INavigationService NavigationService { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public EventCallback OnCreateClicked { get; set; }
        [Parameter] public EventCallback OnBackClicked { get; set; }

        private async Task OnBackClickedAsync()
        {
            if (!OnBackClicked.HasDelegate)
            {
                NavigationService.Back();
            }
            else
            {
                await OnBackClicked.InvokeAsync();
            }
        }
    }
}

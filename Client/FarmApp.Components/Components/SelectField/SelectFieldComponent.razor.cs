using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Properties;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.SelectField
{
    public partial class SelectFieldComponent
    {
        [Inject] private ITabsService TabsService { get; set; } = null!;
        [Inject] public required IStateService StateService { get; set; }
        [Inject] public required INavigationService NavigationService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public string PropertyId { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> PropertyIdChanged { get; set; }

        private bool _isModalOpen = false;

        private List<PropertyViewModel> Properties { get; set; } = new();
        private bool _isLoading;
        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            await Task.WhenAll(StateService.WhenPropertiesReady, Task.Delay(600));
            Properties.AddRange(StateService.Properties);
            _isLoading = false;
        }

        private void OnConfirmPropertyClicked()
        {
            _isModalOpen = false;
        }

        private async Task OnSelectPropertyClicked(string propertyId)
        {
            await PropertyIdChanged.InvokeAsync(propertyId);
        }

        private void OnSelectPropertyClicked()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(PropertyId))
                parameters["PropertyId"] = PropertyId;

            _isModalOpen = true;
        }

        private void NavigateToMap()
        {

            NavigationService.NavigateTo(Constants.ClientRoutes.MainPage, new Dictionary<string, object>
            {
                { "ShowCreateFieldHint", true }
            });

            TabsService.SwitchVisibility(true);
        }
    }
}

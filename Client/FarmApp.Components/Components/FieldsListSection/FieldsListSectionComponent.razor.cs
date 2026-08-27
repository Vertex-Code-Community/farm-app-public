using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.FieldsListSection
{
    public partial class FieldsListSectionComponent
    {
        [Inject] public required INavigationService NavigationService { get; set; }

        [Parameter] public string? SectionTitle { get; set; }

        [Parameter] public required List<PropertyViewModel> FieldsList { get; set; }

        [Parameter] public int StartingAnimationIndex { get; set; } = 1;

        [Parameter] public bool IsLoading { get; set; } = false;

        private void NavigateToMap()
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.MainPage, new Dictionary<string, object>
            {
                { "ShowCreateFieldHint", true }
            });
        }
    }
}

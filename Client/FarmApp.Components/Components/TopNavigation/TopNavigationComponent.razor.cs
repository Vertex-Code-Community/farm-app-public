using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;
namespace FarmApp.Components.Components.TopNavigation
{
    public partial class TopNavigationComponent
    {
        [Inject] public required INavigationService NavigationService { get; set; }

        [Parameter] public string Title { get; set; } = string.Empty;
        [Parameter] public string Subtitle { get; set; } = string.Empty;

        [Parameter] public string BackIconUrl { get; set; } = "_content/FarmApp.Components/img/shared/left-chevron.svg";
        [Parameter] public string IconUrl { get; set; } = string.Empty;

        [Parameter] public string LeftIconUrl { get; set; } = string.Empty;

        [Parameter] public string RightIconUrl { get; set; } = string.Empty;

        [Parameter] public int IconSize { get; set; } = 24;
        [Parameter] public bool IsEditMode { get; set; } = false;
        [Parameter] public bool IsViewMode { get; set; } = false;
        [Parameter] public EventCallback BackCallback { get; set; }
        [Parameter] public EventCallback IconCallback { get; set; }

        protected override void OnParametersSet()
        {
            if (!BackCallback.HasDelegate)
            {
                BackCallback = EventCallback.Factory.Create(this, NavigateBack);
            }
        }

        private void NavigateBack ()
        {
            NavigationService.Back();
        }
    }
}

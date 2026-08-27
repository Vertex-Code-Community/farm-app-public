using FarmApp.Components.Components.Dialogs.Base;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.NotificationDialog
{
    public partial class NotificationDialogComponent : ComponentBase, IBaseDialogComponent<bool?>
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        public EventCallback OnClose { get; set; }
        public EventCallback<bool?> OnSubmit { get; set; }
        public object? Payload { get; set; }

        private string _text = string.Empty;

        protected override void OnInitialized()
        {
            _text = Payload as string ?? Localizer["Error_Default"];
        }

    }
}

using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.CreateNote
{
    public partial class CreateNoteComponent
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public EventCallback OnCreateClicked { get; set; }

        [Parameter] public string? Styles { get; set; }

        [Parameter] required public string Type { get; set; } = "note";

        private string? _title;
        private string? _subtitle;
        private string _iconSrc = "_content/FarmApp.Components/img/shared/leaf.svg";

        protected override void OnParametersSet()
        {
            if (Type == "note")
            {
                _title = Localizer["Property-Note_No-Notes"];
                _subtitle = Localizer["Property-Note_No-Notes_Desc"];
            }

            if (Type == "field")
            {
                _title = Localizer["Field_No-Fields"];
                _subtitle = Localizer["Field_No-Fields_Desc"];
            }

            if (Type == "status")
            {
                _title = Localizer["Status_No-Statuses"];
                _subtitle = Localizer["Status_No-Statuses_Desc"];
                _iconSrc = "_content/FarmApp.Components/img/user-notifications/color-swatch-dark.svg";
            }

            if (Type.Contains("notif"))
            {
                _iconSrc = "_content/FarmApp.Components/img/shared/bell.svg";

                if (Type == "no-notifications")
                {
                    _title = Localizer["Notifications_No-Notifications"];
                    _subtitle = Localizer["Notifications_No-Notifications_Desc"];
                }

                if (Type == "no-new-notifications")
                {
                    _title = Localizer["Notifications_No-New-Notifications"];
                    _subtitle = Localizer["Notifications_No-New-Notifications_Desc"];
                }

                if (Type == "no-read-notifications")
                {
                    _title = Localizer["Notifications_No-Read-Notifications"];
                    _subtitle = Localizer["Notifications_No-Read-Notifications_Desc"];
                }
            }


        }

        private async Task OnCreateClick()
        {
             await OnCreateClicked.InvokeAsync();
        }
    }
}

using FarmApp.Components.Components.Dialogs.Base;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.ConfirmationDialog
{
    public partial class ConfirmationComponent : ComponentBase, IBaseDialogComponent<bool?>
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<bool?> OnSubmit { get; set; }
        [Parameter] public object? Payload { get; set; }

        private string _header = "Deletion";
        private string _questionText = "Confirmation";
        private Dictionary<string, string>? _payloadText;

        protected override void OnParametersSet()
        {
            _payloadText = Payload as Dictionary<string, string>;

            if (_payloadText != null)
            {
                _payloadText.TryGetValue("header", out string? header);
                _payloadText.TryGetValue("body", out string? body);

                _header = header != null ? header : _header;
                _questionText = body != null ? body : _questionText;
            }

        }
    }
}

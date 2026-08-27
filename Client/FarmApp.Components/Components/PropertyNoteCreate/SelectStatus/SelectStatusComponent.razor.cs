using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.PropertyNoteCreate.SelectStatus
{
    public partial class SelectStatusComponent
    {
        [Inject] public required IStringLocalizer<Shared.Resources.Localization.AppRecources> Localizer { get; set; }
        [Parameter, EditorRequired]
        public IReadOnlyCollection<PropertyNoteStatusModel> Statuses { get; set; } = [];

        [Parameter]
        public PropertyNoteStatusModel? Selected { get; set; }

        [Parameter]
        public EventCallback<PropertyNoteStatusModel> OnSelect { get; set; }

        private bool _closeSelect = false;

        private async Task Select(PropertyNoteStatusModel? status)
        {
            await OnSelect.InvokeAsync(status);
            _closeSelect = true;
        }

    }
}

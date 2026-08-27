using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;


namespace FarmApp.Components.Components.StatusBlock
{
    public partial class StatusBlockComponent
    {
        [Inject] public required IStateService StateService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

        [Parameter] public PropertyNoteStatusModel? Status { get; set; }

        [Parameter] public int? StatusId { get; set; }

        [Parameter] public bool Small { get; set; } = false;

        private List<PropertyNoteStatusModel> _statuses = new();
        
        protected override void OnInitialized()
        {
            _statuses = StateService.PropertyNoteStatuses.ToList();
        }
        private string GetStatusName()
        {
            if (Status == null)
                return string.Empty;

            if (Status.IsDefault)
                return Localizer[$"Status_{Status.Code}"];
            else
                return Status.Name;
        }

        protected override void OnParametersSet()
        {
            if (StatusId.HasValue && _statuses.Any())
            {
                Status = _statuses.FirstOrDefault(s => s.Id == StatusId.Value);
            }
        }
    }
}

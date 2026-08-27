using FarmApp.Components.Helpers;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.TimeDropdown
{
    public partial class TimeDropdownComponent
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public string SelectTitle { get; set; } = string.Empty;
        [Parameter] public TimeOptionModel? SelectedTime { get; set; } = null;

        [Parameter] public EventCallback<TimeOptionModel> OnSelect { get; set; }

        private bool _closeDropdown = false;

        private List<TimeOptionModel> _timeOptions = new(10);

        protected override void OnInitialized()
        {
            _timeOptions.AddRange(TimeOptionHelper.GetAll(Localizer));
        }

        private async Task HandleSelect(TimeOptionModel? time)
        {
            await OnSelect.InvokeAsync(time);
            _closeDropdown = true;
        }
    }
}

public class TimeOptionModel
{
    public string Label { get; set; } = string.Empty;
    public NotificationOffset Duration { get; set; }
}

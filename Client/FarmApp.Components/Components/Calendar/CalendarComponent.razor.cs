using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;
namespace FarmApp.Components.Components.Calendar
{
    public partial class CalendarComponent
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public DateTime SelectedDate { get; set; }
        [Parameter] public EventCallback<DateTime> SelectedDateChanged { get; set; }

        private DateTime _viewDate;
        private DateTime _selectedDate;

        private bool _isPickerOpen = false;

        private List<string> _monthsCycleBase = Enumerable.Range(1, 12).Select(i => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)).ToList();
        private List<int> _yearCycleBase = Enumerable.Range(DateTime.Now.Year - 15, 31).ToList();
        private List<string> _daysOfWeek = new() { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };

        private int DaysInMonth => DateTime.DaysInMonth(_viewDate.Year, _viewDate.Month);
        private int PaddingDays => (int)new DateTime(_viewDate.Year, _viewDate.Month, 1).DayOfWeek;
        private string DisplayMonth => _viewDate.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

        private bool _initialized = false;

        private void TogglePicker()
        {
            _isPickerOpen = !_isPickerOpen;
            StateHasChanged();
        }
        protected override void OnInitialized()
        {
            _daysOfWeek = new List<string>
            {
                Localizer["Sunday_Short"], Localizer["Monday_Short"], Localizer["Tuesday_Short"],
                Localizer["Wednesday_Short"], Localizer["Thursday_Short"],Localizer["Friday_Short"],
                Localizer["Saturday_Short"]
            };
        }
        protected override void OnParametersSet()
        {
            _selectedDate = SelectedDate;

            if (!_initialized)
            {
                SyncViewToSelected();
                _initialized = true;
            }
        }

        private async void SelectYear(int year)
        {
            _viewDate = new DateTime(year, _viewDate.Month, _selectedDate.Day, _selectedDate.Hour, _selectedDate.Minute, 0);
            await SelectedDateChanged.InvokeAsync(_viewDate);
        }

        private async void SelectMonth(string monthName)
        {
            int month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;

            int daysInNewMonth = DateTime.DaysInMonth(_viewDate.Year, month);
            int safeDay = Math.Min(_selectedDate.Day, daysInNewMonth);

            _viewDate = new DateTime(_viewDate.Year, month, safeDay, _selectedDate.Hour, _selectedDate.Minute, 0);
            await SelectedDateChanged.InvokeAsync(_viewDate);
        }

        private void SyncViewToSelected()
        {
            _viewDate = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
        }

        private void PreviousMonth() => _viewDate = _viewDate.AddMonths(-1);
        private void NextMonth() => _viewDate = _viewDate.AddMonths(1);

        private async Task SelectDate(int day)
        {
            _selectedDate = new DateTime(_viewDate.Year, _viewDate.Month, day, _selectedDate.Hour, _selectedDate.Minute, 0);
            await SelectedDateChanged.InvokeAsync(_selectedDate);
        }

        private bool IsSelected(int day) =>
            _selectedDate.Year == _viewDate.Year &&
            _selectedDate.Month == _viewDate.Month &&
            _selectedDate.Day == day;
    }
}
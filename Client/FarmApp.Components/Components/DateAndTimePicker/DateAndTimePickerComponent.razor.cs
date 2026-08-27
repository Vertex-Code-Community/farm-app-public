using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.DateAndTimePicker
{
    public partial class DateAndTimePickerComponent
    {
        [Parameter] public string Label { get; set; } = "Label";

        [Parameter] public DateTime SelectedDate { get; set; }
        [Parameter] public EventCallback<DateTime> SelectedDateChanged { get; set; }

        private bool _isCalendarOpen = false;
        private bool _isTimeSelectorOpen = false;

        private DateTime _selectedDateValue;

        private DateTime _selectedDate
        {
            get => _selectedDateValue;
            set
            {
                if (_selectedDateValue != value)
                {
                    _selectedDateValue = value;
                    _ = SelectedDateChanged.InvokeAsync(_selectedDateValue);
                }
            }
        }

        protected override void OnParametersSet()
        {
            _selectedDateValue = SelectedDate;
        }

        private async void ToggleCalendar()
        {
            _isCalendarOpen = !_isCalendarOpen;
            _isTimeSelectorOpen = false;
            await SelectedDateChanged.InvokeAsync(_selectedDate);
            StateHasChanged();
        }

        private async void ToggleTimeSelector()
        {
            _isTimeSelectorOpen = !_isTimeSelectorOpen;
            _isCalendarOpen = false;
            await SelectedDateChanged.InvokeAsync(_selectedDate);
            StateHasChanged();
        }
    }
}
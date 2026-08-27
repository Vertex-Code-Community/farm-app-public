using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.TimeSelect
{
    public partial class TimeSelectComponent
    {
        [Parameter] public DateTime SelectedDate { get; set; }
        [Parameter] public EventCallback<DateTime> SelectedDateChanged { get; set; }

        private DateTime _viewDate;
        private DateTime _selectedDate;

        private List<int> _hourCycleBase = Enumerable.Range(0, 24).ToList();
        private List<int> _minuteCycleBase = Enumerable.Range(0, 60).ToList();

        protected override void OnParametersSet()
        {
            _selectedDate = SelectedDate;
            _viewDate = new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, _selectedDate.Hour, _selectedDate.Minute, 0);
        }

        private async void SelectHour(int hour)
        {
            _viewDate = new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, hour, _viewDate.Minute, 0);
            await SelectedDateChanged.InvokeAsync(_viewDate);
        }

        private async void SelectMinute(int minute)
        {
            _viewDate = new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, _viewDate.Hour, minute, 0);
            await SelectedDateChanged.InvokeAsync(_viewDate);
        }
    }
}
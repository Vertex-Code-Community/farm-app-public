using Bch.Modules.GlobalEvents.Events;
using FarmApp.Components.ViewModels.Calendar;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace FarmApp.Components.Pages.PropertyCalendar.Calendar.MonthFrame;

public partial class MonthFrameComponent : IDisposable
{
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Parameter] public required CultureInfo Culture { get; set; }
    //[Parameter] public PropertyPreviewModel PropertyPreview { get; set; } = new();
    [Parameter] public List<PropertyNoteModel> PropertyNotes { get; set; } = new();
    [Parameter] public string PropertyId { get; set; } = string.Empty;
    [Parameter] public required CalendarComponent CalendarComponent { get; set; }   
    [Parameter] public int Month { get; set; }
    [Parameter] public int Year { get; set; }
    [Parameter] public EventCallback<DateTime?> DateChanged { get; set; }
    [CascadingParameter] public PropertyCalendarPage? PropertyCalendarPage { get; set; }
    [Parameter] public DateTime? SelectedDay { get; set; } = DateTime.Now;
    [Parameter] public EventCallback<DateTime?> SelectedDayChanged { get; set; }
    

    private string _tbodyHeightString = string.Empty;
    private string _tdHeightString = string.Empty;

    // 16 is top padding, 42 and 44 are headers, 4 are margins between headers and table, 26 is thead, 10 is gap to bottom sheet, 160 is closed bottom sheet
    private int _otherElementsOnPage = 16 + 42 + 4 + 44 + 4 + 26 + 10 + 160;

    private readonly List<NameOfDay> _weekDays = new()
    {

    };

    private readonly List<string> _monthNames = new();

    private readonly List<DateTime> _days = new();
    private DateTime _now = DateTime.Now;
    private DateTime? _firstMonthDay;

    private bool _calendarIsOpened;
    private bool _denyClick = false;
    private List<PropertyNoteStatusModel> Statuses = new();
    protected override async Task OnParametersSetAsync()
    {
        _firstMonthDay = new DateTime(Year, Month, 1);
        UpdateCalendar();
        if (SelectedDay is null)
        {
            var today = DateTime.Today;

            if (today.Year == Year && today.Month == Month)
                SelectedDay = today;
            else
                SelectedDay = new DateTime(Year, Month, 1);

            await SelectedDayChanged.InvokeAsync(SelectedDay);
            PropertyCalendarPage?.SetSelectedDay(SelectedDay.Value);
        }
    }

    protected override void OnInitialized()
    {
        var _tbodyHeight = 500.0;
        var _tdHeight = 80.0;
        
        if (OperatingSystem.IsIOS())
        {
            _tbodyHeight = (ScreenOffsetProvider.ScreenHeight / ScreenOffsetProvider.Density)
                - ScreenOffsetProvider.Top - _otherElementsOnPage;
        }

        if (OperatingSystem.IsAndroid())
        {
            _tbodyHeight = (ScreenOffsetProvider.ScreenHeight / ScreenOffsetProvider.Density)
                - ScreenOffsetProvider.Top * 2 - ScreenOffsetProvider.Bottom - _otherElementsOnPage;
        }

        _tdHeight = _tbodyHeight / 6;

        _tbodyHeightString = _tbodyHeight.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        _tdHeightString = _tdHeight.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

        _monthNames.AddRange([
            Localizer["January"],Localizer["February"],Localizer["March"],
            Localizer["April"],Localizer["May"],Localizer["June"],
            Localizer["July"],Localizer["August"],Localizer["September"],
            Localizer["October"],Localizer["November"],Localizer["December"],
            ]);

        _weekDays.AddRange([

            new NameOfDay { Name = Localizer["Monday_Short"], DayOfWeek = DayOfWeek.Monday },
            new NameOfDay { Name = Localizer["Tuesday_Short"], DayOfWeek = DayOfWeek.Tuesday },
            new NameOfDay { Name = Localizer["Wednesday_Short"], DayOfWeek = DayOfWeek.Wednesday },
            new NameOfDay { Name = Localizer["Thursday_Short"], DayOfWeek = DayOfWeek.Thursday },
            new NameOfDay { Name = Localizer["Friday_Short"], DayOfWeek = DayOfWeek.Friday },
            new NameOfDay { Name = Localizer["Saturday_Short"], DayOfWeek = DayOfWeek.Saturday, IsDayOff = true },
            new NameOfDay { Name = Localizer["Sunday_Short"], DayOfWeek = DayOfWeek.Sunday, IsDayOff = true }
            ]);

        Statuses.AddRange(StateService.PropertyNoteStatuses);
        //_firstMonthDay = new DateTime(Year, Month, 1);
        //UpdateCalendar();

        CalendarComponent.TouchStartListener += OnTouchStart;
    }

    public void Dispose()
    {
        CalendarComponent.TouchStartListener -= OnTouchStart;
    }

    private void OnTouchStart(BchTouchEventArgs e)
    {
        var touch = e.Touches.FirstOrDefault();
        if (touch is null) return;
        
        var container = touch.PathCoordinates
            .FirstOrDefault(x => x.ClassList.Contains("bch-calendar-months-modal-wrapper") || 
                                 x.ClassList.Contains("bch-calendar-days-modal-wrapper"));

        _denyClick = _calendarIsOpened && container is null;
        
        Console.WriteLine($"Calendar OnTouchStartAsync _denyClick = {_denyClick}");
    }

    private string GetMonthName(int month)
    {
        return month is >= 1 and <= 12 ? Culture.DateTimeFormat.GetMonthName(month) : string.Empty;
    }

    private void UpdateCalendar()
    {
        _days.Clear();
        _days.AddRange(GetDaysForMonth(Year, Month, Culture));
    }


    private List<DateTime> GetDaysForMonth(int year, int month, CultureInfo cultureInfo)
    {
        var daysList = new List<DateTime>();

        // Step 1: Determine the first day of the specified month.
        var firstDayOfMonth = new DateTime(year, month, 1);

        // Step 2: Find out the day of the week for the first day of the month.
        var firstDayOfWeek = firstDayOfMonth.DayOfWeek;
        
        // Step 3: Calculate the number of days to subtract to get to the first day of the grid, respecting the culture's first day of the week.
        var cultureFirstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
        var daysToSubtract = (7 + (firstDayOfWeek - cultureFirstDayOfWeek)) % 7;
        var firstGridDay = firstDayOfMonth.AddDays(-daysToSubtract);

        // Step 4: Fill the list with DateTime objects for the 7x6 grid (42 days).
        for (var i = 0; i < 42; i++)
        {
            daysList.Add(firstGridDay.AddDays(i));
        }

        return daysList;
    }

    // private static string CapitalizeFirstLetter(string input)
    // {
    //     if (string.IsNullOrEmpty(input))
    //         return input;
    //
    //     // Capitalize the first letter
    //     return char.ToUpper(input[0]) + input.Substring(1);
    // }

    private void OnBackClicked()
    {
        NavigationService.Back();
    }

    [Parameter] public EventCallback<int> onPageChanged { get; set; }

    private void OnArrowClicked(int delta)
    {
        onPageChanged.InvokeAsync(delta);
    }
    private (string bgColor,string textColor)? GetColorsFromStatus(PropertyNoteModel model)
    {
        if (model.StatusId == null)
            return null;

        var status = Statuses.FirstOrDefault(x => x.Id == model.StatusId);

        if (status == null)
            return null;

        return (status.BGColorHex, status.TextColorHex);
    }
    private async Task SelectDate(DateTime date)
    {
        if (_denyClick)
        {
            _denyClick = false;
            return;
        }
        await SelectedDayChanged.InvokeAsync(date);
        if (PropertyCalendarPage != null)
        {
           await PropertyCalendarPage.SetSelectedDay(date);
        }
    }
}
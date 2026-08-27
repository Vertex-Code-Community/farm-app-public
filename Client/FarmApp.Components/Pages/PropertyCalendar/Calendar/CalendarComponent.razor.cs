using Bch.Modules.GlobalEvents.Events;
using Bch.Modules.GlobalEvents.Services;
using FarmApp.Components.Components.SnapSlider;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace FarmApp.Components.Pages.PropertyCalendar.Calendar;

public partial class CalendarComponent : IAsyncDisposable
{
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IGlobalEventsService GlobalEventsService { get; set; }
    [Inject] public required IJSRuntime JS { get; set; }
    
    [Parameter] public DateTime Date { get; set; } = DateTime.Now;
    [Parameter] public DateTime BaseMonth { get; set; } = DateTime.Now;
    [Parameter] public List<PropertyNoteModel> PropertyNotes { get; set; } = new();
    [Parameter] public string PropertyId { get; set; } = string.Empty;
    public DateTime? SelectedDay { get; set; } = null;

    private readonly CultureInfo _culture = CreateCulture();
    private bool _renderHidden = false;
    private int _scrollIndex;
    private DateTime _anchorMonth;
    private bool _iosBaseMonthAdjusted;
    private SnapSliderComponent<DateTime>? _slider;
    
    private readonly string _subscriptionKey = $"_key_{Guid.NewGuid()}";
    
    internal Action<BchTouchEventArgs>? TouchStartListener;

    protected override Task OnInitializedAsync()
    {
        _anchorMonth = new DateTime(Date.Year, Date.Month, 1);
        BaseMonth = _anchorMonth;
        return GlobalEventsService.AddDocumentListenerAsync<BchTouchEventArgs>("touchstart", _subscriptionKey,
            OnTouchStartAsync);
    }

    protected override void OnParametersSet()
    {
        var normalizedDate = new DateTime(Date.Year, Date.Month, 1);
        if (normalizedDate != _anchorMonth)
        {
            _anchorMonth = normalizedDate;
            _scrollIndex = 0;
            BaseMonth = _anchorMonth;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await GlobalEventsService.RemoveDocumentListenerAsync<BchTouchEventArgs>("touchstart", _subscriptionKey);
    }

    private static CultureInfo CreateCulture()
    {
        var culture = new CultureInfo("uk-UA");
        culture.DateTimeFormat.Calendar = new GregorianCalendar();
        return culture;
    }

    private Task OnTouchStartAsync(BchTouchEventArgs e)
    {
        TouchStartListener?.Invoke(e);
        return Task.CompletedTask;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(100);
            _renderHidden = true;
            await InvokeAsync(StateHasChanged);
        }

        if (firstRender && !_iosBaseMonthAdjusted && OperatingSystem.IsIOS())
        {
            _iosBaseMonthAdjusted = true;
            BaseMonth = _anchorMonth;
            StateHasChanged();
        }
    }

    private DateTime OnItem(int shift)
    {
        return _anchorMonth.AddMonths(shift);
    }

    private void OnScrollIndexChanged(int newIndex)
    {
        if (newIndex == _scrollIndex)
            return;

        _scrollIndex = newIndex;
        BaseMonth = _anchorMonth.AddMonths(_scrollIndex);
        SetDateToPageParameters(BaseMonth);
        StateHasChanged();
    }

    private void OnDateChanged(DateTime? dateTime)
    {
        if (dateTime is null) return;
        var date = dateTime.Value;
        Date = date;
        SetDateToPageParameters(date);
        StateHasChanged();
    }

    private void SetDateToPageParameters(DateTime date)
    {
        var currentPage = NavigationService.CurrentPage;
        if (currentPage is null) return;

        currentPage.Parameters["Date"] = date;
    }

    public async Task ChangeMonth(int delta)
    {
        //OnScrollIndexChanged(_scrollIndex + delta);
        /*_scrollIndex += delta;
        BaseMonth = _anchorMonth.AddMonths(_scrollIndex);
        SetDateToPageParameters(BaseMonth);
        StateHasChanged();*/

        if (_slider is null) return;

        await _slider.ScrollAsync(delta > 0);
    }
}

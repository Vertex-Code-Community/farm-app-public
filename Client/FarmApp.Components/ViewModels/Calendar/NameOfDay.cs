namespace FarmApp.Components.ViewModels.Calendar;

public class NameOfDay
{
    public string Name { get; set; } = string.Empty;
    public bool IsDayOff { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}

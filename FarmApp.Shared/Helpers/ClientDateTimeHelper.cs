using System.Text.RegularExpressions;

namespace FarmApp.Shared.Helpers;

public static class ClientDateTimeHelper
{
    public static  string TimeZone
    {
        get => _timeZone;
        set
        {
            _timeZone = value;
            _timeZoneOffset = ParseGmtOffset(_timeZone) ?? new TimeSpan(0, 0, 0, 0);
        }
    }

    public static TimeSpan TimeZoneOffset => _timeZoneOffset;

    private static string _timeZone = "GMT+0000";
    private static TimeSpan _timeZoneOffset = new TimeSpan(0, 0, 0, 0);
    
    public static TimeSpan? ParseGmtOffset(string gmtString)
    {
        var match = Regex.Match(gmtString, @"^GMT(?<sign>[+-])(?<hours>\d{2})(?<minutes>\d{2})$");
        if (!match.Success) return null;

        var hours = int.Parse(match.Groups ["hours"].Value);
        var minutes = int.Parse(match.Groups ["minutes"].Value);
        var sign = match.Groups ["sign"].Value;

        var offset = new TimeSpan(hours, minutes, 0);
        return sign == "+" ? offset : -offset;
    }
}
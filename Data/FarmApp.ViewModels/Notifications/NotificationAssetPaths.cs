namespace FarmApp.ViewModels.Notifications;

public static class NotificationAssetPaths
{
    public const string Root = "_content/FarmApp.Components/img/";

    public static class Shared
    {
        public const string Bell = Root + "shared/bell.svg";
        public const string Leaf = Root + "shared/leaf.svg";
    }

    public static class Weather
    {
        public const string PartlyCloudy = Root + "weather/weather/partly-cloudy.svg";
        public const string Wind = Root + "weather/wind-01.svg";
        public const string Snowflake = Root + "weather/snowflake.svg";
        public const string CloudRain = Root + "weather/cloud-rain.svg";
        public const string Drop = Root + "weather/drop.svg";
        public const string TemperatureHot = Root + "weather/temperature-03.svg";
        public const string TemperatureCold = Root + "weather/temperature-01.svg";
        public const string Sun = Root + "weather/sun-03.svg";
        public const string Cactus = Root + "weather/cactus.svg";
    }

    public static class User
    {
        public const string Refresh = Root + "user-notifications/arrow-refresh-01.svg";
        public const string AlertTriangle = Root + "user-notifications/alert-triangle.svg";
        public const string Tool = Root + "user-notifications/tool.svg";
        public const string CheckBroken = Root + "user-notifications/check-broken.svg";
        public const string ColorSwatch = Root + "user-notifications/color-swatch-dark.svg";
        public const string Alarm = Root + "user-notifications/alarm-01.svg";
        public const string WiltedFlower = Root + "user-notifications/wilted-flower.svg";
    }

    public static class Notes
    {
        public const string Calendar = Root + "notes/calendar-02.svg";
    }

    public static class Tabs
    {
        public const string Map = Root + "tabs/map.svg";
    }
}

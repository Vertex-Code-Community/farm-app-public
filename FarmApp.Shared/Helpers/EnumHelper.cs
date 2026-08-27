using System.Reflection;

namespace FarmApp.Shared.Helpers;

public static class EnumHelper
{
    public static T Parse<T>(string value) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    //public static IList<string> GetDisplayValues<T>() where T : Enum
    //{
    //    return GetNames<T>().Select(obj => Parse<T>(obj).GetDisplayValue()).ToList();
    //}

    public static IList<string> GetNames<T>() where T : Enum
    {
        return typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
    }

    public static IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
}
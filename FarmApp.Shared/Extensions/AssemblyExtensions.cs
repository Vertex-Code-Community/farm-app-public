using System.Reflection;

namespace FarmApp.Shared.Extensions;

public static class AssemblyExtensions
{
    public static List<Type> GetTypesWithAttribute<TAttribute>(this Assembly assembly) where TAttribute : Attribute
    {
        return assembly
            .GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(TAttribute), true).Length > 0)
            .ToList();
    }
}
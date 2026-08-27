namespace Bch.Modules.Themes.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class CssNameAttribute : Attribute
{
    public string CssName { get; }

    public CssNameAttribute(string cssName)
    {
        CssName = cssName;
    }
}



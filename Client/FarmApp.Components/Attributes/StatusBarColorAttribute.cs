namespace FarmApp.Components.Attributes;

public class StatusBarColorAttribute : Attribute
{
    public string Color { get; set; }

    public StatusBarColorAttribute(string color)
    {
        Color = color;
    }
}
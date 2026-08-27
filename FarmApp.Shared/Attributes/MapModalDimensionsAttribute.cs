namespace FarmApp.Shared.Attributes;

public class MapModalDimensionsAttribute : Attribute
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int ArrowRectDepth { get; set; }
    public int ArrowRectLength { get; set; }
}
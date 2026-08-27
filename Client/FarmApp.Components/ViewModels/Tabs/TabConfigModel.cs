namespace FarmApp.Components.ViewModels.Tabs;

public class TabConfigModel
{
    public string Name { get; set; }
    public string ImgUrl { get; set; }
    public string Reference { get; set; }

    public string? ClassName { get; set; }

    public string[] NestedReferences { get; set; }
}

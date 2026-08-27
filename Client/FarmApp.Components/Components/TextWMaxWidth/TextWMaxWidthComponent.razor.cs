using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.TextWMaxWidth
{
    public partial class TextWMaxWidthComponent
    {
        [Parameter] public string MaxWidth { get; set; } = "600px";
        [Parameter] public string TextContent { get; set; } = "Text";
        [Parameter] public string Color { get; set; } = "inherit";
        [Parameter] public string Width { get; set; } = "100%";
    }
}

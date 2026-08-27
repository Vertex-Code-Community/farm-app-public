using Microsoft.AspNetCore.Components;
using System.Net.NetworkInformation;

namespace FarmApp.Components.Components.Button;

public partial class ButtonComponent
{
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public bool Disabled { get; set; } = false;

    [Parameter] public bool Loading { get; set; } = false;

    [Parameter] public string? TextColor { get; set; }

    [Parameter] public string ClassName { get; set; } = "primary";

    [Parameter] public bool HasBorder { get; set; } = true;

    [Parameter] public string? Background { get; set; }
    [Parameter] public string? BorderColor { get; set; }

    [Parameter] public string? IconUrl { get; set; }

    [Parameter] public string? IconColor { get; set; }

    [Parameter] public string Type { get; set; } = "button";

    [Parameter] public string? ExtraStyles { get; set; }
    [Parameter] public bool? CenterRipple { get; set; }
    [Parameter] public string Height { get; set; } = "52px";
    [Parameter] public string Width { get; set; } = "100%";

    private Dictionary<string, object> RippleAttributes => GetRippleAttributes();
    private Dictionary<string, object> GetRippleAttributes()
    {
        var attributes = new Dictionary<string, object>();

        if (CenterRipple == true)
        {
            attributes.Add("ontouchstart", "onRippleEffectTouchStartListener(event, true)");
        } else if (CenterRipple == false)
        {
            attributes.Add("ontouchstart", "onRippleEffectTouchStartListener(event)");
        }

        return attributes;
    }

    protected override void OnParametersSet()
    {
        if (String.IsNullOrEmpty(IconColor))
        {
            if (ClassName == "delete")
            {
                IconUrl = "_content/FarmApp.Components/img/profile/trash-02.svg";
                IconColor = "var(--state-error)";
            }

            if (ClassName == "primary")
            {
                IconColor = "var(--surface-cards)";
            }

            if (ClassName == "secondary")
            {
                IconColor = "var(--text-secondary)";
            }
        }

        if (Disabled == true)
        {
            IconColor = "var(--text-disabled)";
        }
    }
}

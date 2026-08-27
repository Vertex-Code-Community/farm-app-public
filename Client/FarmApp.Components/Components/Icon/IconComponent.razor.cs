using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.Icon
{
    public partial class IconComponent
    {
        [Parameter] public EventCallback IconCallback { get; set; } // Fixed semicolon

        [Parameter] public required string IconSrc { get; set; }

        [Parameter] public int IconContSize { get; set; } = 44;

        [Parameter] public int BorderRadius { get; set; } = 999;

        [Parameter] public int IconSize { get; set; } = 24;

        [Parameter] public string IconColor { get; set; } = "var(--text-secondary)";

        [Parameter] public string IconContBackground { get; set; } = "transparent";

        [Parameter] public int Opacity { get; set; } = 1;

        [Parameter] public string ExtraStyles { get; set; } = string.Empty;

        private bool _isInteractable;

        protected override void OnParametersSet()
        {
            _isInteractable = IconCallback.HasDelegate;

            if (IconContSize < IconSize)
            {
                IconSize = IconContSize;
            }

            if (IconContSize == IconSize && _isInteractable == false)
            {
                BorderRadius = 0;
            }
        }
    }
}
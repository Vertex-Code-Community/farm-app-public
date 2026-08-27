using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.UnorderedList
{
    public partial class UnorderedListComponent
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Parameter] public string BackgroundColor { get; set; } = "var(--surface-cards)";

        [Parameter] public string SeparatorColor { get; set; } = "var(--surface-inputs)";

        [Parameter] public string Height { get; set; } = "100%";

        [Parameter] public string? ExtraStyles { get; set; }

        [Parameter] public string ItemHeight { get; set; } = "52px";

        [Parameter] public int ItemFlexGap { get; set; } = 12;

        [Parameter] public string? ItemExtraStyles { get; set; }

        [Parameter] public bool ItemsInteractable { get; set; } = true;
    }
}

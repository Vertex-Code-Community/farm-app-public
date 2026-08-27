using FarmApp.Components.Components.UnorderedList;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.ListItem
{
    public partial class ListItemComponent
    {
        [CascadingParameter] public UnorderedListComponent? Parent { get; set; }

        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Parameter] public EventCallback ItemCallback { get; set; }
    }
}

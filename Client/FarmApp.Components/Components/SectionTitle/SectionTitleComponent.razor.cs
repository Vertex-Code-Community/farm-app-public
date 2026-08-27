using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.SectionTitle
{
    public partial class SectionTitleComponent
    {
        [Parameter] public string Title { get; set; } = String.Empty;

        [Parameter] public string IconUrl { get; set; } = String.Empty;

        [Parameter] public int IconOpacity { get; set; } = 1;

        [Parameter] public bool IsHeading { get; set; } = false;

        [Parameter] public EventCallback IconCallback { get; set; }
    }
}

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.SkeletonBlock
{
    public partial class SkeletonBlockComponent
    {
        [Parameter] public string Height { get; set; } = "24px";
        [Parameter] public string MaxWidth { get; set; } = "100%";
        [Parameter] public string BorderRadius { get; set; } = "8px";

        private Random rand = new();
    }
}

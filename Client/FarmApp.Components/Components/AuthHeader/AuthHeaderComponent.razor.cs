using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.AuthHeader
{
    public partial class AuthHeaderComponent
    {
        [Parameter] public string Title { get; set; }

        [Parameter] public EventCallback BackCallback { get; set; }
    }
}

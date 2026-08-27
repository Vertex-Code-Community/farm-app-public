using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.Support
{
    public partial class SupportPage
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    }
}

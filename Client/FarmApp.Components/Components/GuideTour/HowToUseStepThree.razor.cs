using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.GuideTour
{
    public partial class HowToUseStepThree
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    }
}

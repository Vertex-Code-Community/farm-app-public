using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.PrivacyPolicy;

public partial class PrivacyPolicyPage
{
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
}
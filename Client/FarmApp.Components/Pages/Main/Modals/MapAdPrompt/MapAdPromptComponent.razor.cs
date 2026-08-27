using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Pages.Main.Modals.MapAdPrompt;

public partial class MapAdPromptComponent
{
    [Parameter] public EventCallback OnAdShowClicked { get; set; }
}
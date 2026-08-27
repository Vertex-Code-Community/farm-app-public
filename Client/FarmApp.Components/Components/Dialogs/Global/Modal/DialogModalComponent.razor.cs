using FarmApp.Services.Models.DialogModels;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.Dialogs.Global.Modal
{
    public partial class DialogModalComponent
    {
        [Parameter] public required DialogParametersModel Parameters { get; set; }
        [Parameter] public required RenderFragment ChildContent { get; set; }
    }
}

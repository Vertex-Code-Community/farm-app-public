using FarmApp.Components.Components.Dialogs.Base;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.MediaChoiceDialog
{
    public partial class MediaChoiceDialogComponent : ComponentBase, IBaseDialogComponent<MediaChoice?>
    {
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback<MediaChoice?> OnSubmit { get; set; }
        [Parameter] public object? Payload { get; set; }
    }
}

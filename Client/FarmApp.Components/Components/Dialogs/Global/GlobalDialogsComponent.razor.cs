using FarmApp.Components.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.Dialogs.Global
{
    public partial class GlobalDialogsComponent
    {
        [Inject] public required IDialogService DialogService { get; set; }

        protected override void OnInitialized()
        {
            DialogService.OnUpdate += StateHasChanged;
        }

        public void Dispose()
        {
            DialogService.OnUpdate -= StateHasChanged;
        }
    }
}

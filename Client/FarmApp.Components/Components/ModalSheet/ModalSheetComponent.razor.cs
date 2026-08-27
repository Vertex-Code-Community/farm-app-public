using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.ModalSheet
{
    public partial class ModalSheetComponent
    {
        [Inject] private ITabsService TabsService { get; set; } = null!;

        [Inject] private IBackButtonService BackButtonService { get; set; } = null!;
        [Parameter] public required RenderFragment ChildContent { get; set; }
        [Parameter] public required RenderFragment OverlaySibling { get; set; }

        [Parameter] public string Title { get; set; } = String.Empty;

        [Parameter] public string TitleIconUrl { get; set; } = "_content/FarmApp.Components/img/shared/check-02.svg";

        [Parameter] public EventCallback TitleIconCallback { get; set; }

        [Parameter] public bool NestedModal { get; set; } = false;

        [Parameter] public bool IsLoading { get; set; } = false;

        // additional top offset in pixels
        [Parameter] public int TopOffset { get; set; } = 0;

        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public bool IsEditMode { get; set; }
        [Parameter] public Func<Task<bool?>>? CanClose { get; set; }
        // used for hiding submit icon
        [Parameter] public bool IsViewMode { get; set; }

        private bool _hadTabs = true;

        private bool _isOpen = false;
        private bool _isClosingCssFlag = false;

        protected override void OnInitialized()
        {
            BackButtonService.OnBackButtonPressed += HandleBackButtonClick;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (IsOpen && !_isOpen)
            {
                _isOpen = true;
                _hadTabs = TabsService.Shown;
                TabsService.SwitchVisibility(false);
            }
            else if (!IsOpen && _isOpen)
            {
                await StartClosingAnimation();
            }
        }

        private async Task StartClosingAnimation()
        {
            _isClosingCssFlag = true;
            StateHasChanged();

            await Task.Delay(400);

            _isOpen = false;
            _isClosingCssFlag = false;

            if (!NestedModal && _hadTabs)
            {
                TabsService.SwitchVisibility(true);
            }
        }

        private async Task HandleBack()
        {
            if (CanClose is not null)
            {
                var allowed = await CanClose.Invoke();
                if (allowed != true)
                    return;
            }

            await StartClosingAnimation();
            await IsOpenChanged.InvokeAsync(false);
        }

        private async Task<bool> HandleBackButtonClick()
        {
            if (!_isOpen) return false;
            await InvokeAsync(async () =>
            {
                await HandleBack();
            });
            return true;
        }

        public void Dispose()
        {
            BackButtonService.OnBackButtonPressed -= HandleBackButtonClick;
        }
    }
}

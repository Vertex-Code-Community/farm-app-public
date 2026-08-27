using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.GuideTour
{
    public partial class GuideTourComponent : ComponentBase, IDisposable
    {
        [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public INavigationService NavigationService { get; set; } = default!;
        [Inject] public IGuideTourService GuideTourService { get; set; } = default!;
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Inject] public required ILocalizationService LocalizationService { get; set; }

        private DotNetObjectReference<GuideTourComponent>? _objRef;

        private IJSObjectReference? _module;

        private ElementReference _guideElRef;

        private GuideStep? _curStep;
        private int _totalSteps = 0;

        protected override void OnInitialized()
        {
            NavigationService.LocationChanged += HandleLocationChanged;
            LocalizationService.LanguageChanged += HandleLanguageChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    _module = await JS.InvokeAsync<IJSObjectReference>("import",
                        "./_content/FarmApp.Components/Components/GuideTour/GuideTourComponent.razor.js");

                    _objRef = DotNetObjectReference.Create(this);

                    GuideTourService.OnTourStateChanged += HandleLocationChanged;

                    GuideTourService.StartTour(TourGroup.Onboarding);

                    await _module.InvokeVoidAsync("init", _objRef);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Module import failed: {ex.Message}");
                }
            }

            _totalSteps = GuideTourService.GetTotalStepsInGroup(GuideTourService.ActiveGroup);

            Console.WriteLine($"ACTIVE STEP IS: {GuideTourService?.ActiveStep?.Sequence} FROM GROUP: {GuideTourService?.ActiveGroup}");

        }

        private void HandleLocationChanged()
        {
            InvokeAsync(async () =>
            {
                if (NavigationService.CurrentPage != null)
                {
                    _curStep = GuideTourService.GetCurrentStepForRoute(NavigationService.CurrentPage.Route);
                    StateHasChanged();

                    if (_module != null)
                    {
                        if (_curStep == null)
                        {
                            await _module.InvokeVoidAsync("stopTour");
                        }
                        else
                        {
                            await Task.Delay(300);
                            if (_curStep != null)
                            {
                                if (_curStep.TargetClicked == true) return;
                                await _module.InvokeVoidAsync("updateSelector", _guideElRef, _curStep.TargetSelector, _curStep.OutlineTarget, _curStep.AllowTargetClick);
                            }
                        }
                    }
                }
            });
        }
        private void HandleLanguageChanged()
        {
            InvokeAsync(() =>
            {
                if (NavigationService.CurrentPage != null)
                {
                    _curStep = GuideTourService.GetCurrentStepForRoute(
                        NavigationService.CurrentPage.Route);

                    StateHasChanged();
                }
            });
        }
        [JSInvokable]
        public void OnTargetClicked()
        {
            if (_curStep == null) return;

            GuideTourService.ClickedCurrentTarget();
        }

        [JSInvokable]
        public void OnSkipClicked()
        {
            GuideTourService.CompleteTour();
        }

        public void Dispose()
        {
            NavigationService.LocationChanged -= HandleLocationChanged;
            LocalizationService.LanguageChanged -= HandleLanguageChanged;
            _objRef?.Dispose();
        }
    }
}

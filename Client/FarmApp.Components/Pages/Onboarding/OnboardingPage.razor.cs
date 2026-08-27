using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace FarmApp.Components.Pages.Onboarding;

public partial class OnboardingPage
{
    [Inject] private IStringLocalizer<AppRecources> Localizer { get; set; } = default!;
    [Inject] public INavigationService NavigationService { get; set; } = default!;

    [Inject] private IAppStoreService StorageService { get; set; } = default!;

    [Inject] private HeightAnimatorService Animator { get; set; } = default!;

    private const string OnboardingFinishedKey = "has_finished_onboarding";

    private OnboardingStep[] _steps = new OnboardingStep[] {};
    private int _currentStepIndex = 0;

    private OnboardingStep _currentStep;

    private int _skipTopOffset = ScreenOffsetProvider.Top + 24;
    private bool _hideImg = false;

    private int _transitionDuration = 125;
    private bool _isTransitioning = false;

    private bool _animateFinalStep = false;

    private ElementReference _animCont;

    protected override void OnInitialized()
    {
        _steps = new OnboardingStep[]
        {
            new (
                Localizer["Onboarding_Step1_Title"],
                Localizer["Onboarding_Step1_Subtitle"],
                new OnboardingImage("_content/FarmApp.Components/img/onboarding/step1.png", "100%", "-180px")
            ),
            new (
                Localizer["Onboarding_Step2_Title"],
                Localizer["Onboarding_Step2_Subtitle"],
                new OnboardingImage("_content/FarmApp.Components/img/onboarding/step2.svg", "350px", "30px")
            ),
            new (
                Localizer["Onboarding_Step3_Title"],
                Localizer["Onboarding_Step3_Subtitle"],
                new OnboardingImage("_content/FarmApp.Components/img/onboarding/step3.svg", "348px", "-83px")
            ),
            new (
                Localizer["Onboarding_Step4_Title"],
                Localizer["Onboarding_Step4_Subtitle"],
                new OnboardingImage("_content/FarmApp.Components/img/onboarding/step4.svg", "329px", "30px")
            ),
            new (
                Localizer["Onboarding_Step5_Title"],
                Localizer["Onboarding_Step5_Subtitle"],
                new OnboardingImage("", "", "")
            )
        };
        _currentStep = _steps[_currentStepIndex];
        StateHasChanged();

    }

    private async Task NextStep()
    {
        if (_isTransitioning) return;

        if (_currentStepIndex < _steps.Length - 1)
        {
            _isTransitioning = true;


            _hideImg = true;
            _currentStepIndex++;
            StateHasChanged();

            await Task.Delay(_transitionDuration);
            await Task.Yield();

            await Animator.TransitionAsync(_animCont, async () =>
            {
                _currentStep = _steps[_currentStepIndex];
                StateHasChanged();
            });
            await Task.Delay(100);

            _hideImg = false;
            StateHasChanged();

            await Task.Delay(_transitionDuration);
            _isTransitioning = false;
        } else { _animateFinalStep = true; }
    }

    private async Task Reset()
    {
        StorageService.SetItem(OnboardingFinishedKey, false);
        _currentStepIndex = 0;
        _currentStep = _steps[_currentStepIndex];
    }

    private void FinishOnboarding()
    {
        StorageService.SetItem(OnboardingFinishedKey, true);
        NavigationService.NavigateTo(Constants.ClientRoutes.HomePage);
    }

}
public record OnboardingImage(string Src, string MaxWidth, string MarginBottom);

public record OnboardingStep(
    string Title,
    string Subtitle,
    OnboardingImage Image
);
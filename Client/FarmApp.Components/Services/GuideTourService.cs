using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Globalization;

namespace FarmApp.Components.Services
{
    public class GuideTourService : IGuideTourService
    {
        private const string CompletedToursKey = "completed_tour_groups";

        private readonly IStringLocalizer<AppRecources> Localizer;
        private readonly ILocalizationService _localizationService;
        private readonly IAppStoreService _storageService;

        public GuideTourService(
                    IStringLocalizer<AppRecources> localizer,
                    ILocalizationService localizationService,
                    IAppStoreService storageService)
        {
            Localizer = localizer;
            _localizationService = localizationService;
            _storageService = storageService;

            _localizationService.LanguageChanged += OnLanguageChanged;

            _steps = InitializeSteps();

            _groupMetadata = new()
        {
            { TourGroup.Onboarding, new() { HasButtons = false } },
            { TourGroup.HowToUse, new() { CompleteAfterSeen = false } },
            { TourGroup.CalendarPage, new() }
        };

            LoadCompletedState();
        }

        public TourGroup ActiveGroup { get; private set; } = TourGroup.None;


        private readonly Dictionary<TourGroup, TourGroupMetadata> _groupMetadata;
        public TourGroupMetadata? ActiveGroupMetadata => GetMetadata(ActiveGroup);

        private TourGroupMetadata? GetMetadata(TourGroup group)
        {
            return _groupMetadata.TryGetValue(group, out var metadata) ? metadata : null;
        }

        public event Action? OnTourStateChanged;

        public GuideStep? ActiveStep { get; private set; } = null;

        public int GetTotalStepsInGroup(TourGroup group)
        {
            var groupSteps = _steps.Where(s => s.Group == group).ToList();

            if (groupSteps.All(s => s.Icon != null))
            {
                return groupSteps.Count;
            }

            return groupSteps.Count(s => s.Icon == null);
        }

        private TourGroup PreviousUnfinishedGroup = TourGroup.None;

        private List<GuideStep> _steps;

        private List<GuideStep> InitializeSteps() => new()
        {
            new GuideStep {
                Group = TourGroup.Onboarding, Sequence = 1,
                Route = Constants.ClientRoutes.HomePage,
                TargetSelector = ".guide-target-create-field-or-note",
                Title = Localizer["First_Guide_Step1_Title"],
                Subtitle = Localizer["First_Guide_Step1_Subtitle"],
            },
            new GuideStep {
                Group = TourGroup.Onboarding, Sequence = 2,
                Route = Constants.ClientRoutes.MainPage,
                TargetSelector = ".farm-button.primary.has-icon.guide-target",
                Title = Localizer["First_Guide_Step2_Title"],
                Subtitle = Localizer["First_Guide_Step2_Subtitle"],
                CompleteOnTargetClick = false,
            },
            new GuideStep {
                Group = TourGroup.Onboarding, Sequence = 3,
                Route = Constants.ClientRoutes.MainPage,
                TargetSelector = ".fields-tab.guide-target",
                Title = Localizer["First_Guide_Step3_Title"],
                Subtitle =  Localizer["First_Guide_Step3_Subtitle"],
            },
            new GuideStep {
                Group = TourGroup.Onboarding, Sequence = 4,
                Route = Constants.ClientRoutes.PropertiesPage,
                TargetSelector = ".field.block",
                Title = Localizer["First_Guide_Step4_Title"],
                Subtitle =  Localizer["First_Guide_Step4_Subtitle"],
            },
            new GuideStep {
                Group = TourGroup.Onboarding, Sequence = 5,
                Route = Constants.ClientRoutes.PropertiesDetailsPage,
                TargetSelector = ".guide-target-create-field-or-note",
                Title = Localizer["First_Guide_Step5_Title"],
                Subtitle =  Localizer["First_Guide_Step5_Subtitle"],
                CompleteOnTargetClick = false,
            },
            new GuideStep {
                Group = TourGroup.Onboarding, Sequence = 6,
                Route = Constants.ClientRoutes.PropertiesDetailsPage,
                TargetSelector = "body",
                Title = Localizer["First_Guide_Step6_Title"],
                Subtitle =  Localizer["First_Guide_Step6_Subtitle"],
                Icon = new StepIconConfig
                {
                    IconSrc = "_content/FarmApp.Components/img/guide-tour/celebrate.svg",
                    IconColor = "original",
                    IconContSize=24,
                    BorderRadius=0,

                }
            },

            new GuideStep {
                Group = TourGroup.HowToUse, Sequence = 1,
                Route = Constants.ClientRoutes.MainPage,
                TargetSelector = ".farm-button.primary.has-icon.guide-target",
                Title = Localizer["HowToUse_Step1_Title"],
                Subtitle = Localizer["HowToUse_Step1_Subtitle"],
            },
            new GuideStep {
                Group = TourGroup.HowToUse, Sequence = 2,
                Route = Constants.ClientRoutes.MainPage,
                TargetSelector = ".choose-a-method-guide",
                Title = Localizer["HowToUse_Step2_Title"],
                Subtitle = Localizer["HowToUse_Step2_Subtitle"],
            },
            new GuideStep {
                Group = TourGroup.HowToUse, Sequence = 3,
                Route = Constants.ClientRoutes.MainPage,
                TargetSelector = ".block.steads.expanded",
                Title = Localizer["HowToUse_Step3_Title"],
            },

            new GuideStep {
                Group = TourGroup.CalendarPage, Sequence = 1,
                Route = Constants.ClientRoutes.PropertyCalendarPage,
                TargetSelector = ".header-cont .icons.right .img-cont:nth-child(2)",
                Title = Localizer["Calendar_Guide_Step1_Title"],
                Subtitle = Localizer["Calendar_Guide_Step1_Subtitle"],
                AllowTargetClick = false,
                OutlineTarget = false,
                Icon = new() {
                    IconSrc = "_content/FarmApp.Components/img/guide-tour/pencil-03.svg",
                    IconSize = 20,
                    IconContSize = 48,
                    IconColor = "var(--component-icon)",
                    IconContBackground = "var(--notification-icon-bg)"
                }
            },
            new GuideStep {
                Group = TourGroup.CalendarPage, Sequence = 2,
                Route = Constants.ClientRoutes.PropertyCalendarPage,
                TargetSelector = ".bottom-sheet-wrapper",
                Title = Localizer["Calendar_Guide_Step2_Title"],
                Subtitle = Localizer["Calendar_Guide_Step2_Subtitle"],
                OutlineTarget = false,
                Icon = new() {
                    IconSrc = "_content/FarmApp.Components/img/guide-tour/list.svg",
                    IconSize = 20,
                    IconContSize = 48,
                    IconColor = "var(--component-icon)",
                    IconContBackground = "var(--notification-icon-bg)"
                }
            }
        };

        private void OnLanguageChanged()
        {
            var currentGroup = ActiveGroup;
            var currentSequence = ActiveStep?.Sequence;
            var currentRoute = ActiveStep?.Route;

            _steps = InitializeSteps();

            if (currentGroup != TourGroup.None && currentSequence != null)
            {
                ActiveStep = _steps.FirstOrDefault(x =>
                    x.Group == currentGroup &&
                    x.Sequence == currentSequence &&
                    x.Route == currentRoute);
            }
            Console.WriteLine(CultureInfo.CurrentUICulture.Name);
            Debug.WriteLine("ZHEST");
            Console.WriteLine(Localizer["Skip"]);
            Console.WriteLine(Localizer["First_Guide_Step1_Title"]);

            NotifyStateChanged();
        }

        public void StartTour(TourGroup group)
        {
            var metadata = GetMetadata(group);
            if (metadata?.Completed == true) return;

            if (ActiveGroup != TourGroup.None)
            {
                PreviousUnfinishedGroup = ActiveGroup;
            }

            ActiveGroup = group;
            foreach (var step in _steps.Where(s => s.Group == group))
                step.IsCompleted = false;

            NotifyStateChanged();
        }

        public void CompleteCurrentStep()
        {
            if (ActiveGroup == TourGroup.None) return;

            if (ActiveStep != null)
            {
                ActiveStep.IsCompleted = true;

                if (!_steps.Any(s => s.Group == ActiveGroup && !s.IsCompleted))
                {
                    CompleteTour();
                }
            }

            NotifyStateChanged();
        }

        public void ClickedCurrentTarget()
        {
            if (ActiveGroup == TourGroup.None) return;

            if (ActiveStep == null) return;

            if (ActiveStep.CompleteOnTargetClick == true) {
                CompleteCurrentStep();
            } else {
              ActiveStep.TargetClicked = true; 
            }

        }

        public void CancelTour()
        {
            ActiveGroup = TourGroup.None;
            NotifyStateChanged();
        }

        public void CompleteTour()
        {
            var metadata = GetMetadata(ActiveGroup);
            if (metadata?.CompleteAfterSeen == true)
            {
                metadata.Completed = true;
                SaveCompletedState();
            }

            ActiveGroup = PreviousUnfinishedGroup;
            PreviousUnfinishedGroup = TourGroup.None;
        }

        public GuideStep? GetCurrentStepForRoute(string route)
        {
            if (ActiveGroup == TourGroup.None) return null;

            var normalizedRoute = route.ToLower().Trim('/');

            var nextRequiredStep = _steps.Where(s => s.Group == ActiveGroup && !s.IsCompleted).OrderBy(s => s.Sequence).FirstOrDefault();

            if (nextRequiredStep != null && nextRequiredStep.Route.ToLower().Trim('/') == normalizedRoute) {
                ActiveStep = nextRequiredStep;
            } else{
                ActiveStep = null;
            }

            return ActiveStep;
        }

        public void Dispose()
        {
            _localizationService.LanguageChanged -= OnLanguageChanged;
        }

        private void NotifyStateChanged() => OnTourStateChanged?.Invoke();

        private void LoadCompletedState()
        {
            var completedGroups = _storageService.GetItem<List<TourGroup>>(CompletedToursKey);

            if (completedGroups == null) return;

            foreach (var group in completedGroups)
            {
                if (_groupMetadata.TryGetValue(group, out var metadata))
                {
                    metadata.Completed = true;
                }
            }
        }

        private void SaveCompletedState()
        {
            var completedGroups = _groupMetadata
                .Where(x => x.Value.Completed)
                .Select(x => x.Key)
                .ToList();

            _storageService.SetItem(CompletedToursKey, completedGroups);
        }
    }
}
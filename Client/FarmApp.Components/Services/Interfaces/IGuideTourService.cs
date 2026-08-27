using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Services.Interfaces
{
    public enum TourGroup { None, Onboarding, HowToUse, CalendarPage }

    public class TourGroupMetadata
    {
        public bool HasButtons { get; set; } = true;
        public bool Completed { get; set; } = false;
        public bool CompleteAfterSeen { get; set; } = true;
    }

    public class GuideStep
    {
        public TourGroup Group { get; set; }
        public int Sequence { get; set; }
        public string Route { get; set; } = string.Empty;
        public string TargetSelector { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public bool CompleteOnTargetClick { get; set; } = true;
        public bool TargetClicked { get; set; } = false;
        public bool AllowTargetClick { get; set; } = true;
        public bool IsCompleted { get; set; }
        public bool OutlineTarget { get; set; } = true;
        public StepIconConfig? Icon { get; set; }
    }

    public class StepIconConfig
    {
        public string IconSrc { get; set; } = string.Empty;
        public int IconSize { get; set; } = 24;
        public int IconContSize { get; set; } = 40;
        public string IconColor { get; set; } = "var(--text-secondary)";
        public string IconContBackground { get; set; } = "transparent";
        public int BorderRadius { get; set; } = 10;
    }

    public interface IGuideTourService
    {
        TourGroup ActiveGroup { get; }
        TourGroupMetadata? ActiveGroupMetadata { get; }

        event Action OnTourStateChanged;
        GuideStep ActiveStep { get; }


        public int GetTotalStepsInGroup(TourGroup group);

        void StartTour(TourGroup group);
        void CompleteCurrentStep();
        void ClickedCurrentTarget();
        void CancelTour();
        void CompleteTour();
        GuideStep? GetCurrentStepForRoute(string route);
    }
}

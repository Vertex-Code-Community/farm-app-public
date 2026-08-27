using FarmApp.Components.Services.Interfaces;

namespace FarmApp.Components.Services
{
    public class MapSearchStateService : IMapSearchStateService
    {
        public float CurrentResultsHeight { get; set; }
        public bool IsOpen { get; set; }
    }
}

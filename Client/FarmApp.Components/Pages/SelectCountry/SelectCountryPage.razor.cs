using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace FarmApp.Components.Pages.SelectCountry
{
    public partial class SelectCountryPage
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Inject] public required ICountryMapStorage CountryMapStorage { get; set; }
        [Inject] public required IPlatformContext PlatformContext { get; set; }
        [Inject] public required IJSRuntime JsRuntime { get; set; }

        private static readonly AppMapCountry[] AllCountries =
        {
            AppMapCountry.Ukraine,
            AppMapCountry.UnitedStates,
            AppMapCountry.Italy,
            AppMapCountry.Poland,
            AppMapCountry.Germany
        };

        private AppMapCountry _selected = AppMapCountry.Ukraine;

        protected override void OnInitialized()
        {
            _selected = CountryMapStorage.Get() ?? AppMapCountry.Ukraine;
        }

        private string LocalizedName(AppMapCountry country) =>
            country switch
            {
                AppMapCountry.Ukraine => Localizer["Country_Ukraine"],
                AppMapCountry.UnitedStates => Localizer["Country_US"],
                AppMapCountry.Italy => Localizer["Country_Italy"],
                AppMapCountry.Poland => Localizer["Country_Poland"],
                AppMapCountry.Germany => Localizer["Country_Germany"],
                _ => country.ToString()
            };

        private static string FlagFile(AppMapCountry country) =>
            country switch
            {
                AppMapCountry.Ukraine => "UA.svg",
                AppMapCountry.UnitedStates => "US.svg",
                AppMapCountry.Italy => "IT.svg",
                AppMapCountry.Poland => "PL.svg",
                AppMapCountry.Germany => "DE.svg",
                _ => "UA.svg"
            };

        private void SelectCountryClicked(AppMapCountry country)
        {
            _selected = country;
            CountryMapStorage.Set(country);
            var (lng, lat, z) = CountryMapViewDefaults.Get(country);
            if (PlatformContext.isWeb)
                _ = ApplyWebMapViewAsync(lng, lat, z);
            else
                IMapCallbackService.FlyTo(lng, lat, z);
        }

        private async Task ApplyWebMapViewAsync(float lng, float lat, float z)
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("farmAppMapApplyView", lng, lat, z);
            }
            catch
            {
                // Web map not initialized
            }
        }
    }
}

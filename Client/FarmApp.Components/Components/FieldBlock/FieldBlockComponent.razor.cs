using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Properties;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;

namespace FarmApp.Components.Components.FieldBlock
{
    public partial class FieldBlockComponent
    {
        [Inject] public required INavigationService NavigationService { get; set; }
        [Parameter] public required PropertyViewModel Property { get; set; }

        [Parameter] public int? AnimationIndex { get; set; }

        private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };

        private string _areaText = String.Empty;
        private bool _isReadyToAnimate = false;

        protected override void OnInitialized()
        {
            _areaText = Math.Round(Property.Area, 4, MidpointRounding.AwayFromZero).ToString(_nF) + "га";

            StateHasChanged();
        }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && AnimationIndex.HasValue)
            {
                _isReadyToAnimate = true;
                StateHasChanged();
            }
        }

        private void NavigateToPropertyDetails(string propertyId)
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.PropertiesDetailsPage, new Dictionary<string, object>
            {
                { "PropertyId", propertyId }
            });
        }

        private void NavigateToPropertyOnMap()
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.MainPage);

            if (Property.Centroid is not null)
                IMapCallbackService.FlyTo((float)Property.Centroid.X, (float)Property.Centroid.Y, Property.Zoom);
        }
    }
}

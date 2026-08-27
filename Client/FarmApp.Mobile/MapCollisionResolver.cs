using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Attributes;
using FarmApp.Shared.Extensions;

namespace FarmApp.Mobile;

public static class MapCollisionResolver
{
    public static bool IsPointerOnMap(IMapModalService mapModalService, IMapSearchStateService searchState, float x, float y)
    {
        var pointerIsOutsideOfBottomPanel = CheckForBottomPanel(x, y);
        var pointerIsOutsideOfModal = CheckForModal(mapModalService, x, y);
        var pointerIsOutsideOfSearchPannel = CheckForSearchBlock(searchState, x, y);
        // Map control buttons: real bounds reported from Blazor (getBoundingClientRect).
        var pointerIsOutsideOfReportedControls = CheckForReportedControls(x, y);

        return pointerIsOutsideOfBottomPanel && pointerIsOutsideOfModal
               && pointerIsOutsideOfSearchPannel
               && pointerIsOutsideOfReportedControls;
    }

    private static bool CheckForReportedControls(float x, float y)
    {
        var cssX = x / ScreenOffsetProvider.Density;
        var cssY = y / ScreenOffsetProvider.Density;

        return !IMapControlsBoundsService.IsPointOnControls(cssX, cssY);
    }

    private static bool CheckForSearchBlock(IMapSearchStateService searchState, float x, float y)
    {
        var searchBoxHeight = 52.0f;
        const int leftRectX = 16;

        var cssX = x / ScreenOffsetProvider.Density;
        var cssY = y / ScreenOffsetProvider.Density;

        var verticalScreenOffsets = OperatingSystem.IsAndroid() ? ScreenOffsetProvider.Top + ScreenOffsetProvider.Bottom : 0;
        var cssWidth = ScreenOffsetProvider.ScreenWidth / ScreenOffsetProvider.Density;
        var cssHeight = (ScreenOffsetProvider.ScreenHeight / ScreenOffsetProvider.Density) - (verticalScreenOffsets);

        if (searchState.IsOpen)
        {
            searchBoxHeight += 4 + searchState.CurrentResultsHeight;
        }

        var searchTopRectY = ScreenOffsetProvider.Top + 16; // 16 is padding of TabLayout
        var searchBottomRectY = searchTopRectY + searchBoxHeight;

        var rightRectX = cssWidth - leftRectX;

        var outsideSearchBar = !CheckIfPointInRectangle(cssX, cssY, leftRectX, searchTopRectY, rightRectX, searchBottomRectY);

        const int howToUseBoxHeight = 37;
        const int howTouseBoxWidth = 114;
        var howToUseTopRectY = searchTopRectY + searchBoxHeight + 8; // 8 is gap to button 
        var howToUseBottomRectY = howToUseTopRectY + howToUseBoxHeight;
        var howToUseRightRectX = leftRectX + howTouseBoxWidth;

        var outisdeHowToUse = !CheckIfPointInRectangle(cssX, cssY, leftRectX, howToUseTopRectY, howToUseRightRectX, howToUseBottomRectY);

        return outsideSearchBar && outisdeHowToUse;
    }

    private static bool CheckForModal(IMapModalService mapModalService, float x, float y)
    {
        var modalType = mapModalService.ModalType;
        if (modalType is null) return true;

        var modalPos = mapModalService.Position;

        var cssX = x / ScreenOffsetProvider.Density;
        var cssY = y / ScreenOffsetProvider.Density;

        var width = modalType.GetValue<int, MapModalDimensionsAttribute>(a => a.Width);
        var height = modalType.GetValue<int, MapModalDimensionsAttribute>(a => a.Height);

        return !CheckIfPointInRectangle(cssX, cssY, modalPos.X, modalPos.Y, modalPos.X + width, modalPos.Y + height);
    }

    private static bool CheckForBottomPanel(float x, float y)
    {
        const int tabBarHeight = 64;
        // TabLayout.razor reserves padding-bottom: safeBottom + 16px + tab bar — that strip is often transparent over the map; taps must not reach MapLibre.
        const int tabLayoutGapAboveTabs = 16;
        const int shadowSlop = 8;
        var footerBlockHeight = tabBarHeight + tabLayoutGapAboveTabs + shadowSlop;

        var cssX = x / ScreenOffsetProvider.Density;
        var cssY = y / ScreenOffsetProvider.Density;

        var verticalScreenOffsets = OperatingSystem.IsAndroid() ? ScreenOffsetProvider.Top + ScreenOffsetProvider.Bottom : 0;
        var cssWidth = ScreenOffsetProvider.ScreenWidth / ScreenOffsetProvider.Density;
        var cssHeight = (ScreenOffsetProvider.ScreenHeight / ScreenOffsetProvider.Density) - (verticalScreenOffsets);

        var topRectY = cssHeight - footerBlockHeight - ScreenOffsetProvider.Bottom;
        var bottomRectY = cssHeight - ScreenOffsetProvider.Bottom;

        // Full width — tab bar is centered with side margins; coordinate drift must not let edge taps hit the map.
        const float leftRectX = 0;
        var rightRectX = cssWidth;

        return !CheckIfPointInRectangle(cssX, cssY, leftRectX, topRectY, rightRectX, bottomRectY);
    }

    private static bool CheckIfPointInRectangle(float pX, float pY, float rectULx, float rectULy,
        float rectLRx, float rectLRy)
    {
        return pX >= rectULx && pX <= rectLRx && pY >= rectULy && pY <= rectLRy;
    }
}
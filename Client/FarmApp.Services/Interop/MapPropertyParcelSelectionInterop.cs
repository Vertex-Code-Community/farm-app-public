using Microsoft.JSInterop;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Interop;

public static class MapPropertyParcelSelectionInterop
{
    [JSInvokable]
    public static void NotifyPropertyParcelSelectionChangedFromJs(int selectionCount)
    {
        IMapPropertyService.SetWebPropertyParcelSelectionCount(selectionCount);
    }
}

using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Components.Pages.Main.Modals.MapCloseVertex;

public partial class MapCloseVertexComponent
{
    private void OnCloseVertexClicked()
    {
        Console.WriteLine("OnCloseVertexClicked CLICK");
        IMapSteadService.InvokeRemoveVertexClicked();
    }

    private void OnMouseDown()
    {
        Console.WriteLine("OnCloseVertexClicked DOWN");
    }
    
    private void OnMouseUp()
    {
        Console.WriteLine("OnCloseVertexClicked UP");
    }
}
namespace FarmApp.Services.Services.Interfaces;

public interface IMapControlsBoundsService
{
    static IReadOnlyList<MapControlBounds> Bounds { get; private set; } = Array.Empty<MapControlBounds>();

    static void SetBounds(IReadOnlyList<MapControlBounds>? bounds) =>
        Bounds = bounds ?? Array.Empty<MapControlBounds>();

    static bool IsPointOnControls(float x, float y)
    {
        var bounds = Bounds;
        for (var i = 0; i < bounds.Count; i++)
            if (bounds[i].Contains(x, y))
                return true;
        return false;
    }
}

public readonly record struct MapControlBounds(float Left, float Top, float Right, float Bottom)
{
    public bool Contains(float x, float y) =>
        x >= Left && x <= Right && y >= Top && y <= Bottom;
}

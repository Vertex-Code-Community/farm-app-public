using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Services;

public class HeaderInsertionService : IHeaderInsertionService
{
    public IReadOnlyList<RenderFragment> Fragments { get; }
    public event Action? OnUpdate;

    private readonly List<RenderFragment> _headerModels = new();

    public HeaderInsertionService()
    {
        Fragments = _headerModels;
    }

    public void Add(RenderFragment renderFragment)
    {
        if (_headerModels.Contains(renderFragment)) return;

        _headerModels.Add(renderFragment);
        OnUpdate?.Invoke();
    }

    public void Remove(RenderFragment headerModel)
    {
        if (!_headerModels.Contains(headerModel)) return;

        _headerModels.Remove(headerModel);
        OnUpdate?.Invoke();
    }

    public void Update()
    {
        OnUpdate?.Invoke();
    }
}

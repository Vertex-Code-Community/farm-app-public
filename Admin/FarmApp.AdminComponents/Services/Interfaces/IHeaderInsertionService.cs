using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IHeaderInsertionService
{
    IReadOnlyList<RenderFragment> Fragments { get; }

    event Action? OnUpdate;

    void Add(RenderFragment renderFragment);
    void Remove(RenderFragment renderFragment);
    void Update();
}

using FarmApp.Shared.Constants.Snackbar;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface ISnackbarService
{
    event Action OnChanged;

    void Show(string message, string colorClass, string position = SnackbarPositions.TopRight, TimeSpan? duration = null, RenderFragment? content = null);

    IReadOnlyList<SnackbarMessage> Messages { get; }

    void Dismiss(string id);
}

public sealed class SnackbarMessage
{
    public required string Id { get; init; }
    public required string Message { get; init; }
    public required string ColorClass { get; init; }
    public required string Position { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public RenderFragment? Content { get; init; }
}

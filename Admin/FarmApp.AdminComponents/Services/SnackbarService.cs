using System.Collections.Concurrent;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Shared.Constants.Snackbar;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Services;

public sealed class SnackbarService : ISnackbarService, IDisposable
{
    private readonly ConcurrentDictionary<string, SnackbarMessage> _messages = new();
    private readonly Timer _sweeperTimer;

    public SnackbarService()
    {
        _sweeperTimer = new Timer(SweepExpired, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public event Action? OnChanged;

    public IReadOnlyList<SnackbarMessage> Messages => _messages.Values
        .OrderBy(static m => m.ExpiresAt)
        .ToList();

    public void Show(
        string message,
        string colorClass,
        string position = SnackbarPositions.TopRight,
        TimeSpan? duration = null,
        RenderFragment? content = null)
    {
        var id = Guid.NewGuid().ToString("N");
        var ttl = duration ?? TimeSpan.FromSeconds(4);
        var snackbar = new SnackbarMessage
        {
            Id = id,
            Message = message,
            ColorClass = colorClass,
            Position = position,
            ExpiresAt = DateTimeOffset.UtcNow.Add(ttl),
            Content = content
        };

        _messages [id] = snackbar;
        OnChanged?.Invoke();
    }

    public void Dismiss(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        _messages.TryRemove(id, out _);
        OnChanged?.Invoke();
    }

    private void SweepExpired(object? _)
    {
        var now = DateTimeOffset.UtcNow;
        var removed = false;
        foreach (var kv in _messages)
        {
            if (kv.Value.ExpiresAt <= now)
            {
                _messages.TryRemove(kv);
                removed = true;
            }
        }

        if (removed)
        {
            OnChanged?.Invoke();
        }
    }

    public void Dispose()
    {
        _sweeperTimer.Dispose();
    }
}

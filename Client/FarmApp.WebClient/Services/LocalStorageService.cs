using Microsoft.JSInterop;
using Newtonsoft.Json;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.WebClient.Services;

public sealed class WebAppStoreService : IAppStoreService
{
    private readonly Dictionary<string, object?> _memory = new();

    public Task<T?> GetItemAsync<T>(string key)
        => Task.FromResult(GetItem<T>(key));

    public T? GetItem<T>(string key)
    {
        if (_memory.TryGetValue(key, out var value))
            return value is T t ? t : default;

        return default;
    }

    public Task SetItemAsync<T>(string key, T value)
    {
        SetItem(key, value);
        return Task.CompletedTask;
    }

    public void SetItem<T>(string key, T value)
    {
        _memory[key] = value;
    }
}

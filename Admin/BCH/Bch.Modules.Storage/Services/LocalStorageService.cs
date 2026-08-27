using Microsoft.JSInterop;

namespace Bch.Modules.Storage.Services;

internal class LocalStorageService(IJSRuntime jsRuntime): ILocalStorageService
{
    public async Task<T> GetItemAsync<T>(string key)
    {
        return await jsRuntime.InvokeAsync<T>("window.localStorage.getItem", key);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        await jsRuntime.InvokeVoidAsync("window.localStorage.setItem", key, value);
    }

    public async Task DeleteItemAsync(string key)
    {
        await jsRuntime.InvokeVoidAsync("window.localStorage.removeItem", key);
    }
    
    public T? GetItem<T>(string key)
    {
        if (jsRuntime is not IJSInProcessRuntime inProcessRuntime)
            throw new NotSupportedException("Synchronous calls are only supported in Blazor WebAssembly.");

        return inProcessRuntime.Invoke<T?>("window.localStorage.getItem", key);
    }

    public void SetItem<T>(string key, T value)
    {
        if (jsRuntime is not IJSInProcessRuntime inProcessRuntime)
            throw new NotSupportedException("Synchronous calls are only supported in Blazor WebAssembly.");

        inProcessRuntime.InvokeVoid("window.localStorage.setItem", key, value);
    }
}
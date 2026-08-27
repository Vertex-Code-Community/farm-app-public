using FarmApp.Services.Services.Interfaces;
using Newtonsoft.Json;

namespace FarmApp.Mobile.Services;

public class LocalStorageService : IAppStoreService
{

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var serializedJson = await SecureStorage.Default.GetAsync(key);
        if (string.IsNullOrWhiteSpace(serializedJson))
            return default;

        return JsonConvert.DeserializeObject<T>(serializedJson);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        
        var serializedValue = JsonConvert.SerializeObject(value);
        await SecureStorage.Default.SetAsync(key, serializedValue);
    }

    public T? GetItem<T>(string key)
    {
        var serializedJson = SecureStorage.Default.GetAsync(key).GetAwaiter().GetResult();
        if (string.IsNullOrWhiteSpace(serializedJson))
            return default;

        return JsonConvert.DeserializeObject<T>(serializedJson);
    }

    public void SetItem<T>(string key, T value)
    {
        var serializedValue = JsonConvert.SerializeObject(value);
        SecureStorage.Default.SetAsync(key, serializedValue).GetAwaiter().GetResult();
    }
}
    
/*    public Task<T?> GetItemAsync<T>(string key)
    {
        return Task.FromResult(GetItem<T>(key));
    }

    public Task SetItemAsync<T>(string key, T value)
    {
        SetItem<T>(key, value);
            
        return Task.CompletedTask;
    }

    public T? GetItem<T>(string key)
    {
        var serializedJson = Preferences.Get(key, string.Empty);
        return JsonConvert.DeserializeObject<T>(serializedJson);
    }

    public void SetItem<T>(string key, T value)
    {
        var serializedValue = JsonConvert.SerializeObject(value);
        Preferences.Set(key, serializedValue);
    }*/

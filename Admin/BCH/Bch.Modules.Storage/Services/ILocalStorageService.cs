namespace Bch.Modules.Storage.Services;

public interface ILocalStorageService
{
    Task<T> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);

    Task DeleteItemAsync(string key);

    T GetItem<T>(string key);

    void SetItem<T>(string key, T value);
}
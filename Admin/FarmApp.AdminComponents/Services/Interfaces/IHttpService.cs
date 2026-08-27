namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IHttpService
{
    public Task<TResult?> PostAsync<TResult, TRequest>(string uriPrefix, TRequest? body, string apiType, IDictionary<string, string>? headers = null)
        where TResult : class, new()
        where TRequest : class;

    public Task<TResult?> PostFormAsync<TResult>(string uriPrefix, MultipartFormDataContent content, string apiType)
        where TResult : class;

    Task<TResult?> GetAsync<TResult>(string uriPrefix, string apiType, IDictionary<string, string>? headers = null)
        where TResult : class, new();

    public Task<TResult?> PutAsync<TResult, TRequest>(string uriPrefix, TRequest? body, string apiType, IDictionary<string, string>? headers = null)
        where TResult : class, new()
        where TRequest : class;

    public Task<TResult?> PutFormAsync<TResult>(string uriPrefix, MultipartFormDataContent content, string apiType)
    where TResult : class;

    public Task<TResult?> DeleteAsync<TResult>(string uriPrefix, string apiType,
        IDictionary<string, string>? headers = null)
        where TResult : class, new();
}
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Models.Options;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Options;
using Microsoft.Extensions.Options;

namespace FarmApp.Mobile.Services;

public class PushNotificationsHttpService : IPushNotificationsHttpService
{
    private readonly IHttpService _httpService;
    private readonly IOptions<PushNotificationOptions> _pushNotificationOptions;

    public PushNotificationsHttpService(
        IHttpService httpService,
        IOptions<PushNotificationOptions> pushNotificationOptions)
    {
        _httpService = httpService;
        _pushNotificationOptions = pushNotificationOptions;
    }

    private IDictionary<string, string> BuildHeaders() => new Dictionary<string, string>
    {
        { nameof(_pushNotificationOptions.Value.ApiKey), _pushNotificationOptions.Value.ApiKey }
    };

    public Task<TResult?> PostAsync<TResult, TRequest>(string uriPrefix, TRequest? body, bool handleAuthError = true)
        where TResult : class, new()
        where TRequest : class
    {
        return _httpService.PostAsync<TResult, TRequest>(
            uriPrefix,
            body!,
            redirectOnUnauthorized: handleAuthError,
            showError: true,
            headers: BuildHeaders());
    }

    public Task<TResult?> DeleteAsync<TResult, TRequest>(string uriPrefix, TRequest? body, bool handleAuthError = true)
        where TResult : class, new()
        where TRequest : class
    {
        return _httpService.DeleteAsync<TResult>(
            uriPrefix,
            redirectOnUnauthorized: handleAuthError,
            showError: true,
            headers: BuildHeaders());
    }

    public Task<TResult?> GetAsync<TResult>(string uriPrefix, bool handleAuthError = true)
        where TResult : class, new()
    {
        return _httpService.GetAsync<TResult>(
            uriPrefix,
            redirectOnUnauthorized: handleAuthError,
            showError: true,
            headers: BuildHeaders());
    }

    public Task<TResult?> PutAsync<TResult, TRequest>(string uriPrefix, TRequest? body, bool handleAuthError = true)
        where TResult : class, new()
        where TRequest : class
    {
        return _httpService.PutAsync<TResult, TRequest>(
            uriPrefix,
            body!,
            redirectOnUnauthorized: handleAuthError,
            showError: true,
            headers: BuildHeaders());
    }
}

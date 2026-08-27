using FarmApp.Services.Auth;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Exceptions;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.UploadPropertyNoteMediaFiles;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static FarmApp.Shared.Constants.Constants;

namespace FarmApp.Services.Services;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly IAppStoreService _appStoreService;
    private readonly AuthStateProvider _authStateProvider;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IPlatformContext _platformContext;
    private readonly TokenRefreshCoordinator _tokenRefreshCoordinator;

    public HttpService(HttpClient httpClient,
        IAppStoreService appStoreService,
        AuthStateProvider authStateProvider,
        INavigationService navigationService,
        INotificationService notificationService,
        IPlatformContext platformContext,
        TokenRefreshCoordinator tokenRefreshCoordinator)
    {
        _httpClient = httpClient;
        _appStoreService = appStoreService;
        _authStateProvider = authStateProvider;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _platformContext = platformContext;
        _tokenRefreshCoordinator = tokenRefreshCoordinator;
    }

    public Task<TResult?> PostAsync<TResult>(string uriPrefix, bool redirectOnUnauthorized = true,
        bool showError = true, CancellationToken cancellationToken = default) where TResult : class, new()
    {
        return PostAsync<TResult, object>(uriPrefix, new object(), redirectOnUnauthorized: redirectOnUnauthorized,
            showError: showError, cancellationToken: cancellationToken);
    }

    private async Task<TResult?> SendAsync<TResult, TRequest>(string uriPrefix, TRequest? body, HttpMethod method,
        bool redirectOnUnauthorized, bool showError, IDictionary<string, string>? headers,
        CancellationToken cancellationToken) where TResult : class, new() where TRequest : class
    {
        uriPrefix = uriPrefix.TrimStart('/');
        var request = new HttpRequestMessage(method, uriPrefix);

        if (body is not null)
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var firstResponse = await SendAsync<TResult>(request, redirectOnUnauthorized, headers, cancellationToken);
        ShowNotificationIfNeeded(firstResponse.Exception, showError);

        if (firstResponse.IsTokenRefreshed)
        {
            request = new HttpRequestMessage(method, uriPrefix);

            if (body is not null)
                request.Content =
                    new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var secondResponse = await SendAsync<TResult>(request, redirectOnUnauthorized, headers, cancellationToken);
            ShowNotificationIfNeeded(secondResponse.Exception, showError);

            return secondResponse.Result;
        }

        return firstResponse.Result;
    }


    private async Task<(TResult? Result, ServerException? Exception, bool IsTokenRefreshed)> SendAsync<TResult>(
        HttpRequestMessage request,
        bool redirectOnUnauthorized = false,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        var token = _appStoreService.GetItem<string>(JwtDetails.ACCESS_TOKEN) ?? string.Empty;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (headers is not null)
        {
            foreach (var kv in headers)
            {
                request.Headers.Remove(kv.Key);
                request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (typeof(TResult) == typeof(object)) return (new TResult(), null, false);
                return (JsonConvert.DeserializeObject<TResult>(content), null, false);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var tokens = await _tokenRefreshCoordinator.RefreshAsync(UpdateRefreshAsync, cancellationToken);

                if (tokens != null) return (null, null, true);

                
                if (!redirectOnUnauthorized) return (null, null, false);

                _appStoreService.SetItem(JwtDetails.ACCESS_TOKEN, string.Empty);
                _authStateProvider.NotifyUserLogout();

                _authStateProvider.MarkSessionExpired();

                _navigationService.History.Clear();
                // _navigationService.History.Add( _navigationService.GetRouteModelByRoute(Constants.ClientRoutes.MainPage)!);
                _navigationService.NavigateTo(ClientRoutes.SessionExpiredPage);



                return (null, null, false);
            }

            var errorJson = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"ERROR {errorJson}");
            var serverException = JsonConvert.DeserializeObject<ServerException>(errorJson);

            var ex2 = serverException ?? new ServerException
            {
                Code = response.StatusCode
            };

            return (null, ex2, false);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine("Operation was canceled!");
            return (null, null, false);
        }
        catch (Exception ex)
        {
            var ex3 = new ServerException
            {
                Code = HttpStatusCode.InternalServerError,
                ErrorMessages = new List<string> { ex.Message }
            };

            return (null, ex3, false);
        }
    }

    public Task<TResult?> PostFormAsync<TResult, TRequest>(string uriPrefix, IReadOnlyCollection<UploadStream> files,
        TRequest body,
        bool redirectOnUnauthorized = true, bool showError = true, CancellationToken cancellationToken = default)
        where TResult : class, new() where TRequest : IMultipartFormRequest
    {
        throw new NotImplementedException();
    }

    public async Task<TokenModel?> UpdateRefreshAsync(CancellationToken cancellationToken = default)
    {
        string? refreshToken = null;
        string? accessToken = _appStoreService.GetItem<string>(Constants.JwtDetails.ACCESS_TOKEN);
        bool isWeb = _platformContext.isWeb;

        if (!isWeb)
        {
            refreshToken = _appStoreService.GetItem<string>(Constants.JwtDetails.REFRESH_TOKEN);
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(accessToken))
                return null;
        }
        else
        {
            if (string.IsNullOrEmpty(accessToken))
                return null;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "api/account/refresh-token");
        request.Content =
            new StringContent(
                JsonConvert.SerializeObject(new TokenModel
                    { AccessToken = accessToken, RefreshToken = refreshToken ?? string.Empty }), Encoding.UTF8,
                "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenModel = JsonConvert.DeserializeObject<TokenModel>(content)!;

            _appStoreService.SetItem(JwtDetails.ACCESS_TOKEN, tokenModel.AccessToken);

            if (!isWeb)
                _appStoreService.SetItem(JwtDetails.REFRESH_TOKEN, tokenModel.RefreshToken);
            await _authStateProvider.NotifyUserUpdateToken();

            return tokenModel;
        }
        catch (Exception ex)
        {
        }

        return null;
    }

    public Task<TResult?> GetAsync<TResult>(string uriPrefix, bool redirectOnUnauthorized = true, bool showError = true,
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        return SendAsync<TResult, object>(uriPrefix, null, HttpMethod.Get, redirectOnUnauthorized, showError, headers,
            cancellationToken);
    }

    public Task<TResult?> DeleteAsync<TResult>(string uriPrefix, bool redirectOnUnauthorized = true,
        bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new()
    {
        return SendAsync<TResult, object>(uriPrefix, null, HttpMethod.Delete, redirectOnUnauthorized, showError,
            headers, cancellationToken);
    }

    public Task<TResult?> PostAsync<TResult, TRequest>(string uriPrefix, TRequest body,
        bool redirectOnUnauthorized = true, bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new() where TRequest : class
    {
        return SendAsync<TResult, TRequest>(uriPrefix, body, HttpMethod.Post, redirectOnUnauthorized, showError,
            headers, cancellationToken);
    }

    public Task<TResult?> PatchAsync<TResult, TRequest>(string uriPrefix, TRequest body,
        bool redirectOnUnauthorized = true, bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new() where TRequest : class
    {
        return SendAsync<TResult, TRequest>(uriPrefix, body, HttpMethod.Patch, redirectOnUnauthorized, showError,
            headers, cancellationToken);
    }

    public Task<TResult?> PutAsync<TResult, TRequest>(string uriPrefix, TRequest body,
        bool redirectOnUnauthorized = true, bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new() where TRequest : class
    {
        return SendAsync<TResult, TRequest>(uriPrefix, body, HttpMethod.Put, redirectOnUnauthorized, showError,
            headers, cancellationToken);
    }

    public Task<TResult?> UploadFileAsync<TResult>(string uriPrefix, UploadStream file,
        bool redirectOnUnauthorized = true, bool showError = true, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        return UploadInternal<TResult, object>(uriPrefix, [file], null, redirectOnUnauthorized, showError,
            cancellationToken);
    }

    public Task<TResult?> UploadFileAsync<TResult, TRequest>(string uriPrefix,
        UploadStream file, TRequest body, bool redirectOnUnauthorized = true,
        bool showError = true, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        return UploadInternal<TResult, TRequest>(uriPrefix, [file], body, redirectOnUnauthorized, showError,
            cancellationToken);
    }

    private async Task<TResult?> UploadInternal<TResult, TRequest>(string uriPrefix,
        IReadOnlyCollection<UploadStream> files, TRequest? body, bool redirectOnUnauthorized = true,
        bool showError = true, CancellationToken cancellationToken = default) where TResult : class, new()
    {
        using var multipartFormDataContent = new MultipartFormDataContent();

        if (body is IMultipartFormRequest multipartFormRequest)
        {
            multipartFormRequest.AddTo(multipartFormDataContent);
        }

        foreach (var file in files)
        {
            var stream = await file.OpenStream();
            Debug.WriteLine($"Uploading {file.FileName}: CanRead={stream.CanRead}, Length={stream.Length}");

            var content = new StreamContent(stream);

            var contentType = file.ContentType;

            if (string.IsNullOrWhiteSpace(contentType) || !contentType.Contains('/'))
            {
                contentType = MediaTypeResolver.GetMimeTypeFromExtension(file.FileName);
            }

            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);


            multipartFormDataContent.Add(content, "file", fileName: file.FileName);
        }


        var request = new HttpRequestMessage(HttpMethod.Post, uriPrefix);

        request.Content = multipartFormDataContent;

        var (result, exception, tokenRefreshed) =
            await SendAsync<TResult>(request, redirectOnUnauthorized, cancellationToken: cancellationToken);

        if (tokenRefreshed)
        {
            return await UploadInternal<TResult, TRequest>(uriPrefix, files, body, redirectOnUnauthorized, showError,
                cancellationToken);
        }

        if (exception != null)
        {
            ShowNotificationIfNeeded(exception, true);
        }


        return result;
    }

    private void ShowNotificationIfNeeded(ServerException? exception, bool showError)
    {
        if (exception is null || !showError) return;
        var message = exception.InnerException is null ? exception.Message : exception.InnerException.Message;

        _notificationService.Add("Помилка", message);
    }
}
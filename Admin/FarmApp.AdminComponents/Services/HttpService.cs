using System.Net.Http.Headers;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models;
using FarmApp.Shared.Helpers;
using Newtonsoft.Json;

namespace FarmApp.AdminComponents.Services;

public class HttpService(
    IHttpClientFactory httpClientFactory,
    IGlobalErrorService globalErrorService,
    IApiOperationCompletionService apiOperationCompletionService)
    : IHttpService
{
    public async Task<TResult?> PostFormAsync<TResult>(
        string uriPrefix,
        MultipartFormDataContent content,
        string apiType) where TResult : class
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(apiType);

            using HttpResponseMessage response = await httpClient.PostAsync(uriPrefix, content);

            apiOperationCompletionService.HandleResponse(HttpMethod.Post, response.StatusCode, uriPrefix);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var exception = JsonConvert.DeserializeObject<Exception>(errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            if (typeof(TResult) == typeof(string))
                return responseContent as TResult;

            return JsonConvert.DeserializeObject<TResult>(responseContent);
        }
        catch (Exception ex)
        {
            globalErrorService.ShowError($"API connection error: {ex.Message}");
            return null;
        }
    }

    public async Task<TResult?> PutFormAsync<TResult>(
        string uriPrefix,
        MultipartFormDataContent content,
        string apiType) where TResult : class
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(apiType);
            using HttpResponseMessage response = await httpClient.PutAsync(uriPrefix, content);
            apiOperationCompletionService.HandleResponse(HttpMethod.Put, response.StatusCode, uriPrefix);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(errorContent);
                globalErrorService.ShowError(apiResponse);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (typeof(TResult) == typeof(string))
                return responseContent as TResult;
            return JsonConvert.DeserializeObject<TResult>(responseContent);
        }
        catch (Exception ex)
        {
            globalErrorService.ShowError($"API connection error: {ex.Message}");
            return null;
        }
    }

    public Task<TResult?> GetAsync<TResult>(
        string uriPrefix,
        string apiType,
        IDictionary<string, string>? headers = null)
        where TResult : class, new()
    {
        Console.WriteLine($"REQUEST: {uriPrefix}");

        var request = new HttpRequestMessage(HttpMethod.Get, uriPrefix);
        return SendAsync<TResult>(request, apiType, headers ?? new Dictionary<string, string>());
    }

    public Task<TResult?> PutAsync<TResult, TRequest>(
        string uriPrefix,
        TRequest? body,
        string apiType,
        IDictionary<string, string>? headers = null) where TResult : class, new() where TRequest : class
    {
        Console.WriteLine($"REQUEST: {uriPrefix}");

        var request = new HttpRequestMessage(HttpMethod.Put, uriPrefix);

        if (body is not null)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        return SendAsync<TResult>(request, apiType, headers ?? new Dictionary<string, string>());
    }

    public Task<TResult?> DeleteAsync<TResult>(
        string uriPrefix,
        string apiType,
        IDictionary<string, string>? headers = null) where TResult : class, new()
    {
        Console.WriteLine($"REQUEST: {uriPrefix}");

        var request = new HttpRequestMessage(HttpMethod.Delete, uriPrefix);

        return SendAsync<TResult>(request, apiType, headers ?? new Dictionary<string, string>());
    }

    public Task<TResult?> PostAsync<TResult, TRequest>(
        string uriPrefix,
        TRequest? body,
        string apiType,
        IDictionary<string, string>? headers = null)
        where TResult : class, new()
        where TRequest : class
    {
        Console.WriteLine($"REQUEST: {uriPrefix}");

        var request = new HttpRequestMessage(HttpMethod.Post, uriPrefix);

        if (body is not null)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        return SendAsync<TResult>(request, apiType, headers ?? new Dictionary<string, string>());
    }

    private async Task<TResult?> SendAsync<TResult>(
        HttpRequestMessage request,
        string apiType,
        IDictionary<string, string> headers,
        Action? onSuccess = null)
        where TResult : class, new()
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(apiType);

            foreach (var keyVal in headers)
                request.Headers.Add(keyVal.Key, keyVal.Value);

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            apiOperationCompletionService.HandleResponse(request.Method, response.StatusCode, request.RequestUri?.ToString());

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(errorContent);
                globalErrorService.ShowError(apiResponse);
                return JsonConvertHelper.DeserializeJsonWithEmptyStringToNull<TResult>(errorContent);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (typeof(TResult) == typeof(string)) return content as TResult;

            try
            {
                return JsonConvert.DeserializeObject<TResult>(content) ?? new();
            }
            catch
            {
                return new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace ?? string.Empty);
            globalErrorService.ShowError($"API connection error: {ex.Message}");
            return null;
        }
    }
}

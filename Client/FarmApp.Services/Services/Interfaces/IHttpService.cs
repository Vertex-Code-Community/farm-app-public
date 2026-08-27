using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.UploadPropertyNoteMediaFiles;

namespace FarmApp.Services.Services.Interfaces;

public interface IHttpService
{
    Task<TResult?> GetAsync<TResult>(string uriPrefix, bool redirectOnUnauthorized = true, bool showError = true,
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        where TResult : class, new();

    Task<TResult?> DeleteAsync<TResult>(string uriPrefix, bool redirectOnUnauthorized = true, bool showError = true,
        IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        where TResult : class, new();

    Task<TResult?> PostAsync<TResult, TRequest>(string uriPrefix, TRequest body, bool redirectOnUnauthorized = true,
        bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new() where TRequest : class;

    Task<TResult?> PatchAsync<TResult, TRequest>(string uriPrefix, TRequest body, bool redirectOnUnauthorized = true,
        bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new() where TRequest : class;

    Task<TResult?> PutAsync<TResult, TRequest>(string uriPrefix, TRequest body, bool redirectOnUnauthorized = true,
        bool showError = true, IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default) where TResult : class, new() where TRequest : class;

    Task<TResult?> UploadFileAsync<TResult>(string uriPrefix, UploadStream file, bool redirectOnUnauthorized = true,
        bool showError = true, CancellationToken cancellationToken = default)
        where TResult : class, new();

    Task<TResult?> UploadFileAsync<TResult, TRequest>(string uriPrefix, UploadStream file, TRequest body,
        bool redirectOnUnauthorized = true, bool showError = true, CancellationToken cancellationToken = default)
        where TResult : class, new();


    Task<TResult?> PostFormAsync<TResult, TRequest>(string uriPrefix, IReadOnlyCollection<UploadStream> files,
        TRequest body, bool redirectOnUnauthorized = true, bool showError = true,
        CancellationToken cancellationToken = default)
        where TResult : class, new()
        where TRequest : IMultipartFormRequest;

    Task<TokenModel?> UpdateRefreshAsync(CancellationToken cancellationToken = default);
}
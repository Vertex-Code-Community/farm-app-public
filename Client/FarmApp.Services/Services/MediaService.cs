using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.Options;
using Microsoft.Extensions.Options;

namespace FarmApp.Services.Services
{
    public class MediaService : IMediaService
    {
        private readonly IHttpService _httpService;
        private readonly ApiOptions _apiOptions;
        public MediaService(IHttpService httpService, 
            IOptions<ApiOptions> apiOptions)
        {
            _httpService = httpService;
            _apiOptions = apiOptions.Value;
        }
        // Upload as temp
        public async Task<TempUploadResult?> UploadTempAsync(UploadStream temp, CancellationToken cancellationToken = default)
        {
            var result = await _httpService.UploadFileAsync<TempUploadResult>("api/media/temp", temp,cancellationToken: cancellationToken);
            return result;   
        }
        // Upload permanently
        public async Task<UploadMediaResult?> UploadMediaAsync(UploadStream file,string propertyNoteId, CancellationToken cancellationToken = default)
        {
            var result = await _httpService.UploadFileAsync<UploadMediaResult>($"api/media/{propertyNoteId}", file, cancellationToken: cancellationToken);
            return result;
        }

        public async Task<SignedUrlResult?> GetSignedMediaUrlAsync(string mediaId, bool thumbnail = false)
        {
            var result = await _httpService.GetAsync<SignedUrlResult>($"api/media/{mediaId}/sign?thumbnail={thumbnail.ToString().ToLower()}");

            return result;
        }
        public string GetThumbnailUrl(string mediaId)
        {
            return GetApiBaseForUrl($"api/media/thumb/{mediaId}");
        } 
        public string GetApiBaseForUrl(string mediaUrl)
        {
            return $"{_apiOptions.BaseUri}{mediaUrl}";
        }

        public async Task<IReadOnlyList<UploadedMediaFile>> GetMediaByNoteIdAsync(string propertyNoteId)
        {
            var result = await _httpService.GetAsync<List<UploadedMediaFile>>($"api/media/note/{propertyNoteId}");
            return (IReadOnlyList<UploadedMediaFile>?)result ?? Array.Empty<UploadedMediaFile>();
        }

        public async Task<DeleteMediaResult?> DeleteUploadedMedia(string propertyNoteId, string mediaId)
        {
            var result = await _httpService.DeleteAsync<DeleteMediaResult>($"api/media/{propertyNoteId}/{mediaId}");
            return result;
        }
    }
}

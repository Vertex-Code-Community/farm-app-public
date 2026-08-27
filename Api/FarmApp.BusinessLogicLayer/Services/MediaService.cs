using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Exceptions;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Http;

namespace FarmApp.BusinessLogicLayer.Services
{
    public class MediaService : IMediaService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IUserService _userService;
        private readonly ISignedUrlService _signedUrlService;
        private readonly IPropertyNoteMediaRepository _propertyNoteMediaRepository;
        private readonly IPropertyNoteRepository _propertyNoteRepository;
        private readonly IPropertyNoteService _propertyNoteService;
        private readonly IImageProcessor _imageProcessor;
        private readonly IVideoThumbnailGenerator _videoThumbnailGenerator;

        private const int ThumbnailSize = 128;

        public MediaService(IFileStorageService fileStorageService,
            IUserService userService,
            ISignedUrlService signedUrlService,
            IPropertyNoteMediaRepository propertyNoteMediaRepository,
            IPropertyNoteService propertyNoteService,
            IImageProcessor imageProcessor,
            IVideoThumbnailGenerator videoThumbnailGenerator,
            IPropertyNoteRepository propertyNoteRepository)
        {
            _fileStorageService = fileStorageService;
            _userService = userService;
            _signedUrlService = signedUrlService;
            _propertyNoteMediaRepository = propertyNoteMediaRepository;
            _propertyNoteService = propertyNoteService;
            _imageProcessor = imageProcessor;
            _videoThumbnailGenerator = videoThumbnailGenerator;
            _propertyNoteRepository = propertyNoteRepository;
        }

        public async Task<List<CommitResult>> CommitAsync(IEnumerable<TempUploadResult> temps, string PropertyNoteId)
        {
            var result = await _fileStorageService.CommitAsync(temps, PropertyNoteId);
            return result;
        }

        public async Task<TempUploadResult> SaveTempAsync(IFormFile file)
        {
            var mediaType = MediaTypeResolver.ResolveMediaType(file);
            if (mediaType == Shared.Enums.MediaType.Unknown)
                throw new ServerException(Constants.ErrorMessages.FILE_IS_NOT_MEDIA);

            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId is null)
                throw new ServerException(Constants.ErrorMessages.USER_DOES_NOT_EXIST);

            using var fileStream = file.OpenReadStream();

            if (fileStream.Length == 0)
                throw new ServerException(Constants.ErrorMessages.FILE_IS_EMPTY);

            await using var thumbnailStream = new MemoryStream();

            await GenerateThumbnailAsync(fileStream, thumbnailStream, mediaType, file.FileName);

            fileStream.Position = 0;

            var result = await _fileStorageService.SaveTempAsync(fileStream, thumbnailStream, file.FileName);
            return result;
        }
        public async Task<UploadMediaResult> SavePermanentMediaAsync(IFormFile file, string propertyNoteId)
        {
            var propertyNote = await _propertyNoteRepository.GetByIdAsync(propertyNoteId);
            if (propertyNote == null)
                throw new ServerException(Constants.ErrorMessages.PROPERTY_NOTE_DOES_NOT_EXISTS);

            var extension = Path.GetExtension(file.FileName);

            var mediaType = MediaTypeResolver.ResolveFromExtension(extension);

            if (mediaType == Shared.Enums.MediaType.Unknown)
            {
                mediaType = MediaTypeResolver.ResolveMediaType(file);
            }
            if (mediaType == Shared.Enums.MediaType.Unknown)
                throw new ServerException(Constants.ErrorMessages.FILE_IS_NOT_MEDIA, System.Net.HttpStatusCode.BadRequest);

            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId is null)
                throw new ServerException(Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

            using var fileStream = file.OpenReadStream();

            if (fileStream.Length == 0)
                throw new ServerException(Constants.ErrorMessages.FILE_IS_EMPTY, System.Net.HttpStatusCode.BadRequest);

            await using var thumbnailStream = new MemoryStream();

            await GenerateThumbnailAsync(fileStream, thumbnailStream, mediaType, file.FileName);

            fileStream.Position = 0;

            var result = await  _fileStorageService.SaveMediaPermanentlyAsync(fileStream, thumbnailStream, propertyNoteId, file.FileName);

            if (result == null)
                throw new ServerException(Constants.ErrorMessages.ERROR_WHILE_SAVING_MEDIA_FILE, System.Net.HttpStatusCode.InternalServerError);

            await _propertyNoteMediaRepository.CreateAsync(new Entities.Entity.PropertyNoteMedia
            {
                Id = result.MediaId,
                MediaType = MediaTypeResolver.ResolveFromExtension(result.RelativePath),
                RelativePath = result.RelativePath,
                ContentType = MediaTypeResolver.GetMimeTypeFromExtension(result.RelativePath),
                PropertyNoteId = propertyNoteId
            });
            bool isPreview = false;
            if (propertyNote.PreviewMediaId == null)
            {
                propertyNote.PreviewMediaId = result.MediaId;
                isPreview = true;
            }

            await _propertyNoteRepository.UpdateAsync(propertyNote);

            var signedUrl = _signedUrlService.GenerateSignedUrls([result.MediaId]).First().Url;

            return new UploadMediaResult
            {
                MediaId = result.MediaId,
                MediaUrl = signedUrl,
                IsPreview = isPreview
            };
        }
        public async Task<string> GetMediaPathById(string mediaId, bool thumbnail = false)
        {
            var media = await _propertyNoteMediaRepository.GetByIdAsync(mediaId);

            var relativePath = media?.RelativePath;
            var path = await _fileStorageService
                .GetMediaPathById(mediaId, relativePath!,thumbnail);

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                throw new ServerException(Constants.ErrorMessages.MEDIA_FILE_DOES_NOT_EXIST, System.Net.HttpStatusCode.NotFound);

             return path;
        }

        public async Task<List<UploadedMediaFile>> GetMediaByPropertyNoteId(string propertyNoteId)
        {
            var userId = _userService.GetCurrentUserId();
            if (userId == null)
                throw new ServerException(Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.NotFound);

            var medias = await _propertyNoteMediaRepository.GetAsync(x => x.PropertyNoteId == propertyNoteId);

            return medias.Select(m =>
            {
                var urlSigned = _signedUrlService.GenerateSignedUrls([m.Id]).First().Url;
                var thumbnailSigned = _signedUrlService.GenerateSignedUrls([m.Id], true).First().Url;

                return new UploadedMediaFile
                {
                    Id = m.Id,
                    ContentType = m.ContentType,
                    MediaType = m.MediaType,
                    ExpiresAt = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds(),
                    PropertyNoteid = m.PropertyNoteId,
                    Url = urlSigned,
                    ThumbnailUrl = thumbnailSigned,
                };
            }).ToList();
        }

        public async Task<DeleteMediaResult> DeleteMedia(string propertyNoteId,string mediaId)
        {
            var propertyNote = await _propertyNoteRepository.GetByIdAsync(propertyNoteId);
            var media = await _propertyNoteMediaRepository.GetByIdAsync(mediaId);

            if (media == null)
                throw new ServerException(Constants.ErrorMessages.MEDIA_FILE_DOES_NOT_EXIST, System.Net.HttpStatusCode.NotFound);

            if (propertyNote == null)
                throw new ServerException(Constants.ErrorMessages.PROPERTY_NOTE_DOES_NOT_EXISTS, System.Net.HttpStatusCode.NotFound);

            if (media.PropertyNoteId != propertyNoteId)
                throw new ServerException(Constants.ErrorMessages.MEDIA_FILE_DOES_NOT_BELONG_TO_PROPERTY_NOTE, System.Net.HttpStatusCode.Forbidden);

            var fileDeleted = await _fileStorageService.DeleteAsync(media.RelativePath);

            if (!fileDeleted)
                throw new ServerException(Constants.ErrorMessages.ERROR_WHILE_DELETING_MEDIA_FILE);

            var result = new DeleteMediaResult();
            await _propertyNoteMediaRepository.DeleteAsync(media);

            if (propertyNote.PreviewMediaId == mediaId)
            {
                var medias = await _propertyNoteMediaRepository.GetAsync(m => m.PropertyNoteId == propertyNoteId);

                var newPreview = medias.FirstOrDefault();

                propertyNote.PreviewMediaId = newPreview?.Id;

                result.IsPreviewDeleted = true;
                result.NewPreviewMediaId = newPreview?.Id;

                await _propertyNoteRepository.UpdateAsync(propertyNote);
            }

            return result;

        }

        private Task GenerateThumbnailAsync(
            Stream fileStream,
            Stream thumbnailStream,
            Shared.Enums.MediaType mediaType,
            string fileName)
        {
            return mediaType == Shared.Enums.MediaType.Video
                ? _videoThumbnailGenerator.CreateThumbnailAsync(fileStream, thumbnailStream, ThumbnailSize, fileName)
                : _imageProcessor.CreateThumbnailAsync(fileStream, thumbnailStream, ThumbnailSize);
        }
    }
}

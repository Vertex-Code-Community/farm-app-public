using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
namespace FarmApp.BusinessLogicLayer.Services
{
    public class LocalFileStorageService : IFileStorageService
    {

        private readonly FileStorageOptions _options;
        private readonly string _contentRootPath;

        public LocalFileStorageService(IOptions<FileStorageOptions> options, IWebHostEnvironment environment)
        {
            _options = options.Value;
            _contentRootPath = environment.ContentRootPath;
        }
        public async Task<TempUploadResult> SaveTempAsync(Stream fileStream,
            Stream thumbnailStream,
            string fileName, 
            CancellationToken ct = default)
        {
            var tempID = Guid.NewGuid().ToString("N");
            var ext = Path.GetExtension(fileName);
            var storedFileName = $"{tempID}{ext}";

            var relativeFolder = Path.Combine("uploads", "temp");
            var relativePath = Path.Combine(relativeFolder, storedFileName);

            var fullPath = Path.Combine(_contentRootPath, relativePath);

            
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await using var fs = File.Create(fullPath);
            await fileStream.CopyToAsync(fs, ct);

            var thumbFileName = $"{tempID}_thumb.jpg";
            var relativeThumbPath = Path.Combine(relativeFolder, thumbFileName);
            var fullThumbPath = Path.Combine(_contentRootPath, relativeThumbPath);

            await using var ts = File.Create(fullThumbPath);
            await thumbnailStream.CopyToAsync(ts, ct);
            return new TempUploadResult
            {
                TempMediaId = tempID,
                TempExtension = ext
            };
        }

        public async Task<CommitResult> SaveMediaPermanentlyAsync(Stream fileStream,
            Stream thumbnailStream, 
            string propertyNoteId, string fileName, 
            CancellationToken ct = default)
        {
            var mediaId = Guid.NewGuid().ToString("N");
            var ext = Path.GetExtension(fileName);
            var storedFileName = $"{mediaId}{ext}";

            var relativeFolder = Path.Combine("uploads", propertyNoteId);
            var relativePath = Path.Combine(relativeFolder, storedFileName);

            var fullPath = Path.Combine(_contentRootPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            await using var fs = File.Create(fullPath);
            await fileStream.CopyToAsync(fs, ct);

            var result = new CommitResult();
            result.MediaId = mediaId;
            result.RelativePath = relativePath;

            var thumbFileName = $"{mediaId}_thumb.jpg";
            var relativeThumbPath = Path.Combine(relativeFolder, thumbFileName);
            var fullThumbPath = Path.Combine(_contentRootPath, relativeThumbPath);

            result.ThumbanailPath = fullThumbPath;

            await using var ts = File.Create(fullThumbPath);
            await thumbnailStream.CopyToAsync(ts, ct);


            return result;
        }
        public Task<List<CommitResult>> CommitAsync(IEnumerable<TempUploadResult> temps, string propertyNoteId)
        {
            var tempDirectory = Path.Combine(_contentRootPath, "uploads", "temp");
            var targetDirectory = Path.Combine(_contentRootPath, "uploads", propertyNoteId);

            Directory.CreateDirectory(targetDirectory);

            var results = new List<CommitResult>();
            foreach(var temp in temps)
            {
                var mediaId = Guid.NewGuid().ToString("N");

                var tempFilePath = Path.Combine(tempDirectory, $"{temp.TempMediaId}{temp.TempExtension}");

                if (!File.Exists(tempFilePath))
                {
                    // Left it as continue
                    // Maybe there should be a thrown exception
                    continue;
                }

                var newFileName = $"{mediaId}{temp.TempExtension}";
                var newFilePath = Path.Combine(targetDirectory, newFileName);
                
                File.Move(tempFilePath, newFilePath);

                var relativePath = Path.Combine("uploads", propertyNoteId, newFileName).Replace("\\", "/");

                string relativeThumbnailPath = string.Empty;

                var tempThumbPath = Path.Combine(tempDirectory, $"{temp.TempMediaId}_thumb.jpg");

                if (File.Exists(tempThumbPath))
                {
                    var newThumbName = $"{mediaId}_thumb.jpg";
                    var newThumbPath = Path.Combine(targetDirectory, newThumbName);

                    File.Move(tempThumbPath, newThumbPath);

                    relativeThumbnailPath = Path.Combine("uploads", propertyNoteId, newThumbName).Replace("\\", "/");
                }

                results.Add(new CommitResult
                {
                    MediaId = mediaId,
                    RelativePath = relativePath,
                    ThumbanailPath = relativeThumbnailPath,
                });
            }
            return Task.FromResult(results);
            
        }

        public Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default)
        {
            var fullPath = Path.Combine(_contentRootPath, relativePath);

            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);
  
        }


        public Task CleanTempFiles()
        {
            var tempPath = Path.Combine(_contentRootPath, "uploads", "temp");

            if (!Directory.Exists(tempPath))
                return Task.CompletedTask;

            var files = Directory.GetFiles(tempPath);

            foreach(var file in files)
            {
                var creationTime = File.GetLastWriteTimeUtc(file);

                if (DateTime.UtcNow -  creationTime > TimeSpan.FromDays(1))
                {
                    File.Delete(file);
                }
            }
            return Task.CompletedTask;
        }

        public Task<string?> GetMediaPathById(string mediaId,string? relativePath, bool thumbnail = false)
        {
            var tempPath = Path.Combine(_contentRootPath, "uploads", "temp");

            string searchPattern = thumbnail ? $"{mediaId}_thumb.jpg" : $"{mediaId}.*";

            var tempFile = Directory
                .GetFiles(tempPath, searchPattern)
                .FirstOrDefault();

            if (tempFile != null)
                return Task.FromResult<string?>(tempFile);

            if (string.IsNullOrEmpty(relativePath))
                return Task.FromResult<string?>(null);

            string finalRelativePath;

            if (!thumbnail)
                finalRelativePath = relativePath;
            else
            {
                var directory = Path.GetDirectoryName(relativePath);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(relativePath);

                finalRelativePath = Path.Combine(directory!, $"{fileNameWithoutExt}_thumb.jpg");
            }
                var fullPath = Path.Combine(_contentRootPath, finalRelativePath);


            if (!File.Exists(fullPath))
                return Task.FromResult<string?>(null);

            return Task.FromResult<string?>(fullPath);
        }


    }
}

using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FarmApp.Api.Controllers
{
    [ApiController]
    [Route("api/media")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        private readonly ISignedUrlService _signedUrlService;
        public MediaController(IMediaService mediaService, ISignedUrlService signedUrlService)
        {
            _mediaService = mediaService;
            _signedUrlService = signedUrlService;
        }

        [HttpPost("temp")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadTemp([FromForm] UploadMediaRequest file)
        {
            var result = await _mediaService.SaveTempAsync(file.File);
            return Ok(result);
        }
        [HttpPost("{propertyNoteId}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMedia([FromForm] UploadMediaRequest file, [FromRoute] string propertyNoteId)
        {
            var result = await _mediaService.SavePermanentMediaAsync(file.File, propertyNoteId);
            return Ok(result);
        }
        [HttpGet("{mediaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedia(string mediaId,
            [FromQuery] string userId, [FromQuery] long expires, [FromQuery] string signature, [FromQuery] bool thumbnail = false)
        {
            var validate = _signedUrlService.Validate(mediaId, userId, expires, signature);
            if (!validate)
                return NotFound();

            var filePath = await _mediaService.GetMediaPathById(mediaId, thumbnail);

            var contentType = GetContentType(filePath);

            return PhysicalFile(filePath, contentType, true);
        }
        [HttpGet("thumb/{mediaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetThumbMedia(string mediaId)
        {
            var filePath = await _mediaService.GetMediaPathById(mediaId, true);

            var contentType = GetContentType(filePath);

            return PhysicalFile(filePath, contentType, true);
        }

        [Authorize]
        [HttpGet("{mediaId}/sign")]
        public IActionResult GetMediaSign(string mediaId,[FromQuery] bool thumbnail = false)
        {
            var result = _signedUrlService.GenerateSignedUrls([mediaId], thumbnail).First();

            return Ok(result);
        }
        [HttpGet("sign")]
        [Authorize]
        public IActionResult GetMediaSigns(IEnumerable<string> mediaIds, [FromQuery] bool thumbnail = false)
        {
            var result = _signedUrlService.GenerateSignedUrls(mediaIds, thumbnail);

            return Ok(result);
        }

        [HttpGet("note/{propertyNoteId}")]
        [Authorize]
        public async Task<IActionResult> GetMediaByPropertyNoteId(string propertyNoteId)
        {
            var result = await _mediaService.GetMediaByPropertyNoteId(propertyNoteId);
            return Ok(result);
        }
        [HttpDelete("{propertyNoteId}/{mediaId}")]
        public async Task<IActionResult> DeleteMedia(string propertyNoteId,string mediaId)
        {
            var result = await _mediaService.DeleteMedia(propertyNoteId, mediaId);
            return Ok(result);
        }
        private static string GetContentType(string path)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

            return provider.TryGetContentType(path, out var type)
                ? type
                : "application/octet-stream";
        }

    }
}

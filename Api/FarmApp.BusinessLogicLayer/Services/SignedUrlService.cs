using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Shared.Exceptions;
using FarmApp.ViewModels.Media;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace FarmApp.BusinessLogicLayer.Services
{
    public class SignedUrlService : ISignedUrlService
    {
        private readonly IUserService _userService;
        private readonly byte[] _secret;
        private long _expires;
        public SignedUrlService(IUserService userService, IConfiguration configuration)
        {
            var secret = configuration["MediaSigningSecret"];
            _secret = Encoding.UTF8.GetBytes(secret!);

            _userService = userService;
            _expires = DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds();
        }
        public IEnumerable<SignedUrlResult> GenerateSignedUrls(IEnumerable<string> mediaIds, bool thumbnail = false)
        {
            var userId = _userService.GetCurrentUserId();

            if (userId == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

            foreach(var mediaId in mediaIds)
            {
                var hash = Generate(mediaId, userId, _expires);
                yield return new SignedUrlResult
                {
                    MediaId = mediaId,
                    Url = $"/api/media/{mediaId}?userId={userId}&expires={_expires}&signature={hash}&thumbnail={thumbnail}"
                };
            }

        }
        public bool Validate(string mediaId, string userId, long exprires, string signature)
        {
            var expected = Generate(mediaId, userId, exprires);

            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));
            
        }

        private string Generate(string mediaId, string userId, long exprires)
        {
            var payload = $"{mediaId}:{userId}:{exprires}";

            using var hmac = new HMACSHA256(_secret);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

            return Convert.ToHexString(hash);
        }
    }
}

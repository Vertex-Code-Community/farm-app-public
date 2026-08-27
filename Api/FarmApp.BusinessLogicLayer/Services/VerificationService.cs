using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Models;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Accounts;
using FarmApp.ViewModels.Email;
using FarmApp.ViewModels.Options;
using FarmApp.ViewModels.Verifications;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using static FarmApp.Shared.Constants.Constants;
namespace FarmApp.BusinessLogicLayer.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly EmailOptions _emailOptions;
        private readonly EmailProvider _emailProvider;
        private readonly VerificationOptions _verificationOptions;
        private readonly IMemoryCache _memoryCache;
        public VerificationService(IOptions<EmailOptions> emailOptions, 
            EmailProvider emailProvider, 
            IOptions<VerificationOptions> verificationOptions,
            IMemoryCache memoryCache)
        {
            _emailOptions = emailOptions.Value;
            _emailProvider = emailProvider;
            _verificationOptions = verificationOptions.Value;
            _memoryCache = memoryCache;
        }

        private string GetKey(string email, VerificationPurpose purpose)
            => $"verification_{purpose}_{email}";
        private string GetRateKey(string email, VerificationPurpose purpose)
            => $"verification_{purpose}_{email}_rate";
        public async Task<ApiResponse> ResendCodeAsync(string email, VerificationPurpose purpose)
        {
            var key = GetKey(email, purpose);
            var cached = _memoryCache.Get<VerificationCacheModel>(key);

            if (cached == null)
                return await SendCodeAsync(email, purpose, payload: null)
                    ? ApiResponses.Ok()
                    : ApiResponses.Error(ErrorMessages.CODE_IS_NOT_SENT);

            var rateKey = GetRateKey(email, purpose);

            if (_memoryCache.TryGetValue(rateKey, out var rate))
                return ApiResponses.Ok();

            var newCode = GenerateNumericCode(4);

            var result = await SendCodeAsync(email, newCode);

            if (!result)
                return ApiResponses.Error(ErrorMessages.CODE_IS_NOT_SENT);

            cached.CodeHash = SecurityHelper.Sha256(newCode);

            _memoryCache.Set(key, cached, TimeSpan.FromMinutes(_verificationOptions.Ttl));
            _memoryCache.Set(rateKey, true, TimeSpan.FromSeconds(60));

            return ApiResponses.Ok();

        }

        public async Task<bool> SendCodeAsync(string email, VerificationPurpose purpose, object? payload)
        {
            var rateCache = _memoryCache.Get(GetRateKey(email, purpose));

            var existing = _memoryCache.Get<VerificationCacheModel>(GetKey(email, purpose));

            if (rateCache != null && existing != null)
                return true;

            var code = GenerateNumericCode(4);

            var model = existing ?? new VerificationCacheModel
            {
                Email = email,
                Payload = payload
            };

            model.CodeHash = SecurityHelper.Sha256(code);

            _memoryCache.Set(GetKey(email, purpose), model,TimeSpan.FromMinutes(_verificationOptions.Ttl));
            _memoryCache.Set(GetRateKey(email, purpose), true, TimeSpan.FromSeconds(60));

            return await SendCodeAsync(email, code);
        }

        public ApiResponse VerifyCode(string email, string code, VerificationPurpose purpose)
        {
            var cached = _memoryCache.Get<VerificationCacheModel>(GetKey(email, purpose));

            if (cached == null)
                return ApiResponses.Error(ErrorMessages.CODE_EXPIRED);

            var inputHash = SecurityHelper.Sha256(code);

            if (!string.Equals(inputHash, cached.CodeHash, StringComparison.Ordinal))
            {
                return ApiResponses.Error(ErrorMessages.WRONG_CODE);
            }

            return ApiResponses.Ok();
        }
        public object? GetPayload(string email, VerificationPurpose purpose)
        {
            var cached = _memoryCache.Get<VerificationCacheModel>(GetKey(email, purpose));
            return cached?.Payload;
        }
        public void Remove(string email, VerificationPurpose purpose)
            => _memoryCache.Remove(GetKey(email, purpose));
        private async Task<bool> SendCodeAsync(string email, string verificationCode)
        {
            var emailSendResult = await _emailProvider.SendMailAsync(
            new EmailModel
            {
                EmailToId = email,
                EmailSubject = "FarmApp verification code",
                EmailBody =
                    $"Your verification code: <b>{verificationCode}</b><br/>This code will expire in {_verificationOptions.Ttl} minutes.",
                EmailFromName = "FarmApp"
            },
            new EmailConnectionOptions
            {
                Server = _emailOptions.Server,
                Port = _emailOptions.Port,
                SenderName = _emailOptions.SenderName,
                SenderEmail = _emailOptions.SenderEmail,
                UserName = _emailOptions.UserName,
                Password = _emailOptions.Password
            });

            return emailSendResult;
        }
        private static string GenerateNumericCode(int length)
        {
            var maxExclusive = (int)Math.Pow(10, length);
            var value = Random.Shared.Next(0, maxExclusive);
            return value.ToString(new string('0', length));
        }
    }
}

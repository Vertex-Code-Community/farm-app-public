using FarmApp.ViewModels.Media;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface ISignedUrlService
    {
        IEnumerable<SignedUrlResult> GenerateSignedUrls(IEnumerable<string> mediaIds, bool thumbnail = false);
        bool Validate(string mediaId, string userId, long exprires, string signature);
    }
}

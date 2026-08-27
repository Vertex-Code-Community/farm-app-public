using FarmApp.ViewModels.Email;

namespace FarmApp.Services.Services.Interfaces;

public interface IMessageService
{
    Task<ContactUsResultModel?> ContactUsAsync(ContactUsModel model, CancellationToken cancellationToken = default);
}

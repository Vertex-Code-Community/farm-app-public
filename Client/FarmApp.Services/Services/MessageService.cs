using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Email;

namespace FarmApp.Services.Services;

public class MessageService : IMessageService
{
    private readonly IHttpService _httpService;

    public MessageService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<ContactUsResultModel?> ContactUsAsync(ContactUsModel model, CancellationToken cancellationToken = default)
    {
        return await _httpService.PostAsync<ContactUsResultModel, ContactUsModel>("api/message/contact-us", model, cancellationToken: cancellationToken);
    }
}

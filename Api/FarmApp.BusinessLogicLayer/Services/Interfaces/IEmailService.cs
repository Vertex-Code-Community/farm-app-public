using FarmApp.Models.Email;
using FarmApp.ViewModels.Email;
using FarmApp.ViewModels.Options;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface IEmailService
{
    Task<ContactUsResultModel> ContactUsAsync(ContactUsModel model);
    Task<bool> SendEmailAsync(SendEmailRequest model);
}

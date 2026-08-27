using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Models.Email;
using FarmApp.ViewModels.Email;
using FarmApp.ViewModels.Options;
using Microsoft.Extensions.Options;

namespace FarmApp.BusinessLogicLayer.Services;

public class EmailService : IEmailService
{
    private readonly EmailProvider _emailProvider;
    private readonly EmailOptions _emailOptions;
    private readonly EmailConnectionOptions _options;

    public EmailService(
        IOptions<EmailConnectionOptions> options,
        EmailProvider emailProvider,
        IOptions<EmailOptions> emailOptions)
    {
        _options = options.Value;
        _emailProvider = emailProvider;
        _emailOptions = emailOptions.Value;
    }

    public async Task<ContactUsResultModel> ContactUsAsync(ContactUsModel model)
    {
        var requestModel = new EmailModel
        {
            EmailBody = model.Message,
            EmailSubject = "farm-app@gmail.com",
            EmailToId = _emailOptions.SenderEmail,
            EmailFromName = model.FromApp ? "Mobile Application" : "Web site"
        };

        var connectionOptions = new EmailConnectionOptions
        {
            Server = _emailOptions.Server,
            Port = _emailOptions.Port,
            SenderName = _emailOptions.SenderName,
            SenderEmail = _emailOptions.SenderEmail,
            UserName = _emailOptions.UserName,
            Password = _emailOptions.Password
        };

        var result = await _emailProvider.SendMailAsync(requestModel, connectionOptions);
        return new ContactUsResultModel { IsSuccess = result };
    }

    public Task<bool> SendEmailAsync(SendEmailRequest model)
    {
        return _emailProvider.SendMailAsync(new EmailModel
        {
            EmailToId = model.EmailTo,
            EmailFromName = "FarmApp",
            EmailSubject = model.EmailSubject,
            EmailBody = model.EmailBody,
            EmailRecipients = model.EmailRecipients
        }, _options);
    }
}

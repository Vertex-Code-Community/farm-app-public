using FarmApp.ViewModels.Email;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;

namespace FarmApp.BusinessLogicLayer.Providers;

public class EmailProvider
{
	public async Task<bool> SendMailAsync(EmailModel mailData, EmailConnectionOptions emailConnectionOptions)
	{
		Console.WriteLine($"\n mailData");
		Console.WriteLine(JsonConvert.SerializeObject(mailData));
		Console.WriteLine("_____________________________________");
		Console.WriteLine($"\n emailConnectionOptions");
		Console.WriteLine(JsonConvert.SerializeObject(emailConnectionOptions));

		try
		{
			using var emailMessage = new MimeMessage();
			var emailFrom = new MailboxAddress(emailConnectionOptions.SenderName, emailConnectionOptions.SenderEmail);
			emailMessage.From.Add(emailFrom);

			if (!string.IsNullOrEmpty(mailData.EmailToId))
			{
				var emailTo = new MailboxAddress(mailData.EmailFromName, mailData.EmailToId);
				emailMessage.To.Add(emailTo);
			}

			if (mailData.EmailRecipients.Any())
			{
				foreach (var recipient in mailData.EmailRecipients)
				{
					emailMessage.To.Add(new MailboxAddress(recipient, recipient));
				}
			}

			emailMessage.Subject = mailData.EmailSubject;

			var emailBodyBuilder = new BodyBuilder();
			emailBodyBuilder.HtmlBody = mailData.EmailBody;

			emailMessage.Body = emailBodyBuilder.ToMessageBody();
			using var mailClient = new SmtpClient();
			mailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;

			await mailClient.ConnectAsync(emailConnectionOptions.Server, emailConnectionOptions.Port, MailKit.Security.SecureSocketOptions.Auto);
			await mailClient.AuthenticateAsync(emailConnectionOptions.UserName, emailConnectionOptions.Password);
			var response = await mailClient.SendAsync(emailMessage);
			await mailClient.DisconnectAsync(true);

			Console.WriteLine($"send email response = {response}");

			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);

			return false;
		}
	}
}

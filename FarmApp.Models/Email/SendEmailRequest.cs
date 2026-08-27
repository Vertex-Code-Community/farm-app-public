namespace FarmApp.Models.Email;

public class SendEmailRequest
{
    public required string? EmailTo { get; set; }
    public required string EmailSubject { get; set; }
    public required string EmailBody { get; set; }

    public List<string> EmailRecipients { get; set; } = [];
}
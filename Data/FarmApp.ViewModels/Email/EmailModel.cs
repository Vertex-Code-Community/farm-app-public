namespace FarmApp.ViewModels.Email;

public class EmailModel
{
	public string EmailToId { get; set; } = string.Empty;
	public List<string> EmailRecipients { get; set; } = new();
	public string EmailSubject { get; set; } = string.Empty;
	public string EmailBody { get; set; } = string.Empty;
	public string EmailFromName { get; set; } = string.Empty;
}



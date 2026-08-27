namespace FarmApp.Shared.Options;

public class DevCredentialsOptions
{
    public const string SectionName = "DevCredentials";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

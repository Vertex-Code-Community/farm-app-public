namespace FarmApp.ViewModels.Options;

public class JwtConnectionOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int LifetimeRefreshToken { get; set; }
    public int LifetimeAccessToken { get; set; }
    public int LengthRefreshToken { get; set; }
    public string SecretKey { get; set; }
}
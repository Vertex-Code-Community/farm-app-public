using System.Text;
using System.Text.Json;
using Bch.Modules.Storage.Services;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models;
using FarmApp.Models.User;
using FarmApp.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Services;

public class AuthenticationService(
    IHttpService httpService,
    ILocalStorageService localStorage,
    NavigationManager navigationManager)
    : IAuthenticationService
{
    private const string AccessTokenKey = "access-token";

    public bool IsUserLoggedIn => TryGetStoredToken(out var token) && !IsTokenExpired(token);


    public async Task<UserTokenModel?> LoginAsync(CredentialsModel model)
    {
        var loginResponse = await httpService.PostAsync<UserTokenModel, CredentialsModel>
            ("api/auth/login", model, ApiType.ManagementAppApi);

        if (loginResponse is not null)
        {
            await localStorage.SetItemAsync(AccessTokenKey, loginResponse.AccessToken);
            navigationManager.NavigateTo(Constants.ClientRoutesAdmin.WelcomePage);
        }

        return loginResponse;
    }

    public async Task LogoutAsync()
    {
        await localStorage.DeleteItemAsync(AccessTokenKey);
        navigationManager.NavigateTo(Constants.ClientRoutesAdmin.Login);
    }

    private bool TryGetStoredToken(out string token)
    {
        token = localStorage.GetItem<string>(AccessTokenKey) ?? string.Empty;
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<string?> GetToken()
    {
        var token = await localStorage.GetItemAsync<string?>(AccessTokenKey);

        if (string.IsNullOrWhiteSpace(token))
            return null;

        if (IsTokenExpired(token))
        {
            await LogoutAsync();
            return null;
        }

        return token;
    }

    private static bool IsTokenExpired(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return true;

            var payload = parts[1];
            var jsonBytes = Base64UrlDecode(payload);
            var payloadJson = Encoding.UTF8.GetString(jsonBytes);

            using var doc = JsonDocument.Parse(payloadJson);
            if (!doc.RootElement.TryGetProperty("exp", out var expElement)) return true;

            long expSeconds = expElement.ValueKind switch
            {
                JsonValueKind.Number => expElement.GetInt64(),
                // Safety check in case exp is passed as a string
                JsonValueKind.String => long.TryParse(expElement.GetString(), out var v) ? v : 0,
                _ => 0
            };

            if (expSeconds <= 0) return true;

            var exp = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
            return DateTimeOffset.UtcNow >= exp;
        }
        catch
        {
            return true;
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }

        return Convert.FromBase64String(s);
    }
}
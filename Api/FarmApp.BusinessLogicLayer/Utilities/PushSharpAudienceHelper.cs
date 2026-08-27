using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using PushSharp.Net;

namespace FarmApp.BusinessLogicLayer.Utilities;

/// <summary>
/// Resolves tag expressions (same syntax as the former hub installations) to raw device tokens using <see cref="IDeviceRegistrationStore"/>.
/// </summary>
public static class PushSharpAudienceHelper
{
    public static async Task<List<string>> ResolveDeviceTokensAsync(
        IDeviceRegistrationStore store,
        NotificationModel model,
        CancellationToken cancellationToken)
    {
        var expression = NotificationTagExpressionBuilder.Build(model)
                         ?? BuildFallbackUserTagExpression(model);

        var tokenSet = await ResolveTokensAsync(store, expression, model.TypeOfTargetUser, cancellationToken)
            .ConfigureAwait(false);

        return FilterPlatform(tokenSet, model.Platform);
    }

    private static string? BuildFallbackUserTagExpression(NotificationModel model)
    {
        if (!model.Tags.TryGetValue(NotificationTagsType.User, out var users) || users.Count == 0)
            return null;

        var parts = users
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => $"{NotificationTagExpressionBuilder.UserShopperIdTagPrefix}{u.Trim()}")
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return parts.Count switch
        {
            0 => null,
            1 => parts[0],
            _ => $"({string.Join(" || ", parts)})"
        };
    }

    private static async Task<HashSet<string>> ResolveTokensAsync(
        IDeviceRegistrationStore store,
        string? tagExpression,
        string targetUserType,
        CancellationToken cancellationToken)
    {
        if (string.Equals(targetUserType, TargetUserType.All, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(tagExpression))
        {
            var all = await store.GetAllTokensAsync(cancellationToken).ConfigureAwait(false);
            return new HashSet<string>(all, StringComparer.Ordinal);
        }

        if (string.IsNullOrWhiteSpace(tagExpression))
            return new HashSet<string>(StringComparer.Ordinal);

        var conjuncts = tagExpression.Split("&&", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        HashSet<string>? acc = null;

        foreach (var conjunct in conjuncts)
        {
            var inner = conjunct.Trim();
            while (inner.Length >= 2 && inner.StartsWith('(') && inner.EndsWith(')'))
                inner = inner[1..^1].Trim();

            var orParts = inner.Split("||", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeTagToken)
                .Where(t => t.Length > 0)
                .ToList();

            var union = new HashSet<string>(StringComparer.Ordinal);
            foreach (var tag in orParts)
            {
                foreach (var t in await store.GetTokensByTagAsync(tag, cancellationToken).ConfigureAwait(false))
                    union.Add(t);
            }

            acc = acc is null
                ? union
                : new HashSet<string>(acc.Intersect(union), StringComparer.Ordinal);
        }

        return acc ?? new HashSet<string>(StringComparer.Ordinal);
    }

    private static string NormalizeTagToken(string raw)
    {
        var s = raw.Trim();
        while (s.Length > 0 && (s[0] == '(' || s[0] == ' '))
            s = s.TrimStart('(', ' ');
        while (s.Length > 0 && (s[^1] == ')' || s[^1] == ' '))
            s = s.TrimEnd(')', ' ');
        return s.Trim();
    }

    private static List<string> FilterPlatform(IEnumerable<string> tokens, string platform) =>
        tokens.Where(t => platform switch
            {
                Platform.IOS => IsApnsToken(t),
                Platform.Android => !IsApnsToken(t),
                _ => true
            })
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static bool IsApnsToken(string token) =>
        token.Length == 64 && token.All(Uri.IsHexDigit);
}

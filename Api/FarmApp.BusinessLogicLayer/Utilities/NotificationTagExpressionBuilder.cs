using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;

namespace FarmApp.BusinessLogicLayer.Utilities;

public static class NotificationTagExpressionBuilder
{
    public const string UserShopperIdTagPrefix = "user_shopper_id:";
    public const string VersionTagPrefix = "version:";

    private static readonly Dictionary<NotificationTagsType, string> Prefix = new()
    {
        { NotificationTagsType.User, UserShopperIdTagPrefix },
        { NotificationTagsType.VersionOwner, VersionTagPrefix },
    };

    public static string? Build(NotificationModel model)
    {
        var andParts = new List<string>();

        foreach (var tagPair in model.Tags)
        {
            if (!Prefix.TryGetValue(tagPair.Key, out _))
                continue;

            if (tagPair.Value is null || tagPair.Value.Count == 0)
                continue;

            var values = tagPair.Value
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Prefix[tagPair.Key] + x.Trim())
                .Distinct()
                .ToList();

            if (values.Count == 0)
                continue;

            andParts.Add(values.Count == 1 ? values[0] : $"({string.Join(" || ", values)})");
        }

        if (andParts.Count == 0)
            return null;

        return string.Join(" && ", andParts);
    }

    public static List<string> ToEntityTags(NotificationModel model)
    {
        var result = new List<string>();

        if (model.Tags is not null)
        {
            foreach (var tagPair in model.Tags)
            {
                if (!Prefix.TryGetValue(tagPair.Key, out var key))
                    continue;

                if (tagPair.Value is null || tagPair.Value.Count == 0)
                    continue;

                foreach (var raw in tagPair.Value)
                {
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;

                    var value = raw.Trim();
                    result.Add($"{key}{value}");
                }
            }
        }

        return result
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static Dictionary<NotificationTagsType, HashSet<string>> FromEntityTags(List<string>? tags)
    {
        var dict = new Dictionary<NotificationTagsType, HashSet<string>>();

        if (tags is null || tags.Count == 0)
            return dict;

        foreach (var raw in tags)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var t = raw.Trim();
            if (t.StartsWith(UserShopperIdTagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var value = t[UserShopperIdTagPrefix.Length..].Trim();
                Add(dict, NotificationTagsType.User, value);
            }
            else if (t.StartsWith(VersionTagPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var value = t[VersionTagPrefix.Length..].Trim();
                Add(dict, NotificationTagsType.VersionOwner, value);
            }
        }

        return dict;

        static void Add(Dictionary<NotificationTagsType, HashSet<string>> d, NotificationTagsType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (!d.TryGetValue(type, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                d[type] = set;
            }

            set.Add(value);
        }
    }
}

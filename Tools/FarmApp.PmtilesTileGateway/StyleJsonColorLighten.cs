using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace FarmApp.PmtilesTileGateway;

/// <summary>
/// Walks a MapLibre/Mapbox <c>style.json</c> tree and lightens dark color literals on a whitelist
/// of "stroke-like" paint properties (text, halo, lines, icon outlines). Background fills and area
/// fill-color values are intentionally left dark so the dark theme keeps its dark look.
/// </summary>
internal static class StyleJsonColorLighten
{
    private static readonly Regex Hex6 = new(@"^#([0-9a-fA-F]{6})$", RegexOptions.Compiled);
    private static readonly Regex Hex3 = new(@"^#([0-9a-fA-F]{3})$", RegexOptions.Compiled);
    private static readonly Regex HslLike = new(
        @"^hsla?\(\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)%?\s*,\s*(-?\d+(?:\.\d+)?)%?\s*(?:,\s*(-?\d+(?:\.\d+)?%?)\s*)?\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex RgbLike = new(
        @"^rgba?\(\s*(\d+(?:\.\d+)?)\s*,\s*(\d+(?:\.\d+)?)\s*,\s*(\d+(?:\.\d+)?)\s*(?:,\s*(-?\d+(?:\.\d+)?%?)\s*)?\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> LightenableKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "text-color",
        "text-halo-color",
        "line-color",
        "icon-color",
        "icon-halo-color",
        "circle-color",
        "circle-stroke-color"
    };

    /// <param name="mixTowardWhite">0 = no change, ~0.15–0.3 = noticeably softer dark theme.</param>
    public static void ApplyDarkTheme(JsonObject root, double mixTowardWhite, double? darkOnlyMaxChannelAvg = 96)
    {
        if (mixTowardWhite is <= 0 or >= 1) return;
        var maxAvg = darkOnlyMaxChannelAvg ?? 255;
        ProcessNode(root, mixTowardWhite, maxAvg);
    }

    private static void ProcessNode(JsonNode? node, double mix, double maxAvgForFullBlend)
    {
        switch (node)
        {
            case JsonObject o:
            {
                foreach (var p in o.ToList())
                {
                    if (LightenableKeys.Contains(p.Key)
                        && p.Value is JsonValue v
                        && v.TryGetValue<string>(out var s)
                        && IsColorLiteral(s))
                    {
                        if (LightenIfWorthIt(s, mix, maxAvgForFullBlend, out var next))
                            o[p.Key] = next;
                    }
                    else
                        ProcessNode(p.Value, mix, maxAvgForFullBlend);
                }

                break;
            }
            case JsonArray a:
            {
                for (var i = 0; i < a.Count; i++) ProcessNode(a[i], mix, maxAvgForFullBlend);
                break;
            }
        }
    }

    private static bool IsColorLiteral(string s) =>
        (s.Length is >= 4 and <= 7 && s[0] == '#')
        || s.StartsWith("hsl(", StringComparison.OrdinalIgnoreCase)
        || s.StartsWith("hsla(", StringComparison.OrdinalIgnoreCase)
        || s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase)
        || s.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase);

    private static bool LightenIfWorthIt(
        string s,
        double baseMix,
        double maxChannelAvg,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? next)
    {
        next = null;

        if (TryParseHsl(s, out var hsl, out var hslAlpha))
        {
            if (hsl.l > maxChannelAvg / 255.0 * 100) return false;

            var hslT = baseMix * (1 + (1 - hsl.l / 100) * 0.35);
            if (hslT > 0.5) hslT = 0.5;

            var newL = hsl.l + (100 - hsl.l) * hslT;
            var hStr = hsl.h.ToString(CultureInfo.InvariantCulture);
            var sStr = hsl.s.ToString("0.##", CultureInfo.InvariantCulture);
            var lStr = newL.ToString("0.##", CultureInfo.InvariantCulture);
            next = hslAlpha is null
                ? $"hsl({hStr}, {sStr}%, {lStr}%)"
                : $"hsla({hStr}, {sStr}%, {lStr}%, {hslAlpha})";
            return true;
        }

        if (TryParseRgb(s, out var rgb, out var rgbAlpha))
        {
            var avgRgb = (rgb.r + rgb.g + rgb.b) / 3.0;
            if (avgRgb > maxChannelAvg) return false;

            var tRgb = baseMix * (1 + (1 - avgRgb / 255) * 0.35);
            if (tRgb > 0.5) tRgb = 0.5;

            var nr = Blend(rgb.r, tRgb);
            var ng = Blend(rgb.g, tRgb);
            var nb = Blend(rgb.b, tRgb);
            next = rgbAlpha is null
                ? $"rgb({nr}, {ng}, {nb})"
                : $"rgba({nr}, {ng}, {nb}, {rgbAlpha})";
            return true;
        }

        if (!TryParse(s, out var r, out var g, out var b)) return false;

        var avg = (r + g + b) / 3.0;
        if (avg > maxChannelAvg) return false;

        // Darker base colors get a bit more lift so land/background don't stay near black.
        var t = baseMix * (1 + (1 - avg / 255) * 0.35);
        if (t > 0.5) t = 0.5;

        r = Blend(r, t);
        g = Blend(g, t);
        b = Blend(b, t);
        next = $"#{r:X2}{g:X2}{b:X2}";
        return true;
    }

    private static int Blend(int c, double t) => (int)Math.Min(255, Math.Round(c + (255 - c) * t));

    private static bool TryParseHsl(string s, out (double h, double s, double l) hsl, out string? alpha)
    {
        hsl = (0, 0, 0);
        alpha = null;
        var m = HslLike.Match(s);
        if (!m.Success) return false;

        hsl.h = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        hsl.s = double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
        hsl.l = double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
        if (m.Groups[4].Success) alpha = m.Groups[4].Value.Trim();
        return true;
    }

    private static bool TryParseRgb(string s, out (int r, int g, int b) rgb, out string? alpha)
    {
        rgb = (0, 0, 0);
        alpha = null;
        var m = RgbLike.Match(s);
        if (!m.Success) return false;

        rgb.r = Clamp255(double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture));
        rgb.g = Clamp255(double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
        rgb.b = Clamp255(double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
        if (m.Groups[4].Success) alpha = m.Groups[4].Value.Trim();
        return true;
    }

    private static int Clamp255(double v) => (int)Math.Clamp(Math.Round(v), 0, 255);

    private static bool TryParse(string s, out int r, out int g, out int b)
    {
        r = g = b = 0;
        var m6 = Hex6.Match(s);
        if (m6.Success)
        {
            r = int.Parse(m6.Groups[1].Value.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            g = int.Parse(m6.Groups[1].Value.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            b = int.Parse(m6.Groups[1].Value.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return true;
        }

        var m3 = Hex3.Match(s);
        if (!m3.Success) return false;
        var t = m3.Groups[1].Value;
        r = int.Parse(t.AsSpan(0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture) * 17;
        g = int.Parse(t.AsSpan(1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture) * 17;
        b = int.Parse(t.AsSpan(2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture) * 17;
        return true;
    }
}

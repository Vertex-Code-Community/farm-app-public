using Bch.Modules.Themes.Attributes;

namespace Bch.Modules.Themes.Models;

/// <summary>
/// Supported UI themes for BCH components.
/// </summary>
public enum BchTheme
{
    /// <summary>
    /// Light silver monochrome theme.
    /// </summary>
    [CssName("bch-theme-light")]
    Light = 0,

    /// <summary>
    /// Current default light green theme.
    /// </summary>
    [CssName("bch-theme-light-green")]
    LightGreen = 1,

    /// <summary>
    /// Dark theme.
    /// </summary>
    [CssName("bch-theme-dark")]
    Dark = 2
}



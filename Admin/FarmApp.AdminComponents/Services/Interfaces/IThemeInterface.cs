using Bch.Modules.Themes.Models;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IThemeInterface
{
    /// <summary>
    /// Raised when the active theme changes. Carries the new theme name (e.g., "light", "dark", "system").
    /// </summary>
    event Action<string>? OnThemeChanged;

    /// <summary>
    /// The current theme identifier (e.g., "light", "dark", "system").
    /// </summary>
    string CurrentTheme { get; }

    /// <summary>
    /// Strongly-typed current theme for BCH components.
    /// </summary>
    BchTheme CurrentBchTheme { get; }

    /// <summary>
    /// Convenience flag indicating whether the effective theme is dark.
    /// </summary>
    bool IsDark { get; }

    /// <summary>
    /// Initializes theme state (e.g., reading stored preference and applying it).
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Sets the theme explicitly (e.g., "light", "dark", or "system").
    /// </summary>
    Task SetThemeAsync(string theme);

    /// <summary>
    /// Toggles between light and dark themes. Implementations may decide how this interacts with "system".
    /// </summary>
    Task ToggleThemeAsync();
}

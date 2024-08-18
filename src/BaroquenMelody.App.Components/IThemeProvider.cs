using MudBlazor;

namespace BaroquenMelody.App.Components;

public interface IThemeProvider
{
    MudTheme Theme { get; }

    bool IsDarkMode { get; }

    void ToggleDarkMode();

    string DarkLightModeButtonIcon { get; }

    Color DarkLightModeIconColor { get; }

    string DarkLightTooltipText { get; }

    double TooltipDelay { get; }

    double TooltipDuration { get; }
}

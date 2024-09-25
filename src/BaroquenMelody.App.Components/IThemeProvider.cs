using MudBlazor;

namespace BaroquenMelody.App.Components;

public interface IThemeProvider
{
    MudTheme Theme { get; }

    bool IsDarkMode { get; set; }

    void ToggleDarkMode();

    string DarkLightModeButtonIcon { get; }

    Color DarkLightModeIconColor { get; }

    string DarkLightModeTooltipText { get; }

    double TooltipDelay { get; }

    double TooltipDuration { get; }

    public int Elevation { get; }
}

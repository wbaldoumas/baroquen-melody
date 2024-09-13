﻿using BaroquenMelody.App.Components;
using MudBlazor;

namespace BaroquenMelody.App.Infrastructure.Theme;

internal sealed class MauiThemeProvider : IThemeProvider
{
    private static readonly PaletteLight _lightPalette = new()
    {
        Black = "#110e2d",
        AppbarText = "#424242",
        AppbarBackground = "rgba(255,255,255,0.8)",
        DrawerBackground = "#ffffff",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9",
        Success = "#1ec8a5"
    };

    private static readonly PaletteDark _darkPalette = new()
    {
        Primary = "#7e6fff",
        Surface = "#1e1e2d",
        Background = "#1a1a27",
        BackgroundGray = "#151521",
        AppbarText = "#92929f",
        AppbarBackground = "rgba(26,26,39,0.8)",
        DrawerBackground = "#1a1a27",
        ActionDefault = "#74718e",
        ActionDisabled = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",
        TextPrimary = "#b2b0bf",
        TextSecondary = "#92929f",
        TextDisabled = "#ffffff33",
        DrawerIcon = "#92929f",
        DrawerText = "#92929f",
        GrayLight = "#2a2833",
        GrayLighter = "#1e1e2d",
        Info = "#4a86ff",
        Success = "#1ec8a5",
        Warning = "#ffb545",
        Error = "#ff3f5f",
        LinesDefault = "#33323e",
        TableLines = "#33323e",
        Divider = "#292838",
        OverlayLight = "#1e1e2d80"
    };

    public MudTheme Theme { get; } = new()
    {
        PaletteLight = _lightPalette,
        PaletteDark = _darkPalette,
        LayoutProperties = new LayoutProperties()
    };

    public bool IsDarkMode
    {
        get => Preferences.Get("IsDarkMode", true);
        private set => Preferences.Set("IsDarkMode", value);
    }

    public string DarkLightModeButtonIcon => IsDarkMode switch
    {
        true => Icons.Material.Rounded.LightMode,
        false => Icons.Material.Rounded.DarkMode
    };

    public MudBlazor.Color DarkLightModeIconColor => IsDarkMode switch
    {
        true => MudBlazor.Color.Warning,
        false => MudBlazor.Color.Primary
    };

    public string DarkLightModeTooltipText => IsDarkMode switch
    {
        true => "Switch to Light mode",
        false => "Switch to Dark mode"
    };

    public double TooltipDelay => 250;

    public double TooltipDuration => 250;

    public void ToggleDarkMode() => IsDarkMode = !IsDarkMode;
}

using BaroquenMelody.App.Components;

namespace BaroquenMelody.App;

/// <summary>
///     The entrypoint of the application.
/// </summary>
public partial class App : Application
{
    private readonly IThemeProvider _themeProvider;

    public App(IThemeProvider themeProvider)
    {
        InitializeComponent();

        _themeProvider = themeProvider;

        MainPage = new MainPage();
        MainPage.Title = "Baroquen Melody";
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState) ?? throw new InvalidOperationException("Window is null");

        window.Title = "Baroquen Melody";

        window.Created += (_, _) =>
        {
            _themeProvider.IsDarkMode = Preferences.Default.Get("IsDarkMode", true);
        };

        window.Destroying += (_, _) =>
        {
            Preferences.Default.Set("IsDarkMode", _themeProvider.IsDarkMode);
        };

        return window;
    }
}

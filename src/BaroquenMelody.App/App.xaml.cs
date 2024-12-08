using BaroquenMelody.App.Components;

namespace BaroquenMelody.App;

/// <summary>
///     The entrypoint of the application.
/// </summary>
public partial class App : Application
{
    private const string Title = "Baroquen Melody";

    private const string IsDarkModeKey = "IsDarkMode";

    private readonly IThemeProvider _themeProvider;

    public App(IThemeProvider themeProvider)
    {
        InitializeComponent();

        _themeProvider = themeProvider;

        _themeProvider.IsDarkMode = Preferences.Default.Get(IsDarkModeKey, defaultValue: true);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage { Title = Title })
        {
            Title = Title
        };

        window.Destroying += (_, _) => { Preferences.Default.Set(IsDarkModeKey, _themeProvider.IsDarkMode); };

        return window;
    }
}

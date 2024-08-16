using BaroquenMelody.Library.Compositions.Configurations.Services;

namespace BaroquenMelody.App;

/// <summary>
///     The entrypoint of the application.
/// </summary>
public partial class App : Application
{
    public App(ICompositionConfigurationService compositionConfigurationService)
    {
        InitializeComponent();

        compositionConfigurationService.ConfigureDefaults();

        MainPage = new MainPage();
    }
}

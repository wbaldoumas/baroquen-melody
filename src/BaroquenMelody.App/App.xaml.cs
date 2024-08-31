namespace BaroquenMelody.App;

/// <summary>
///     The entrypoint of the application.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }
}

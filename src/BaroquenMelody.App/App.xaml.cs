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
        MainPage.Title = "Baroquen Melody";
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);

        if (window is not null)
        {
            window.Title = "Baroquen Melody";
        }

        return window ?? throw new InvalidOperationException("Window is null");
    }
}

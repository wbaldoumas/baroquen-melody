using BaroquenMelody.App.Components;
using BaroquenMelody.App.Components.Extensions;
using BaroquenMelody.App.Infrastructure.FileSystem;
using BaroquenMelody.App.Infrastructure.Theme;
using BaroquenMelody.Library.Infrastructure.FileSystem;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace BaroquenMelody.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<IMidiLauncher, MauiMidiLauncher>();
        builder.Services.AddSingleton<IMidiSaver, MauiMidiSaver>();
        builder.Services.AddSingleton<IThemeProvider, MauiThemeProvider>();
        builder.Services.AddBaroquenMelodyComponents();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("MyBlazorCustomization", (handler, view) =>
        {
#if IOS
            handler.PlatformView.Opaque = false;
            handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
#elif WINDOWS
            handler.PlatformView.Opacity = 0;
            handler.PlatformView.DefaultBackgroundColor = new Windows.UI.Color() { A = 0, R = 0, G = 0, B = 0 };
#endif
        });

        return builder.Build();
    }
}

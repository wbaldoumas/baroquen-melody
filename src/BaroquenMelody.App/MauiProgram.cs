﻿using BaroquenMelody.App.Components;
using BaroquenMelody.App.Components.Extensions;
using BaroquenMelody.App.Infrastructure.Application;
using BaroquenMelody.App.Infrastructure.Devices;
using BaroquenMelody.App.Infrastructure.FileSystem;
using BaroquenMelody.App.Infrastructure.Theme;
using BaroquenMelody.Infrastructure.Application;
using BaroquenMelody.Infrastructure.Devices;
using BaroquenMelody.Library.Midi;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.WebView.Maui;
#pragma warning disable IDE0005 // Using directive is unnecessary.
using Microsoft.Extensions.Logging;
#pragma warning restore IDE0005 // Using directive is unnecessary.

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
        builder.Services.AddSingleton<IPhysicalDeviceInfo, MauiDeviceInfo>();
        builder.Services.AddSingleton<IMidiLauncher, MauiMidiLauncher>();
        builder.Services.AddSingleton<IMidiSaver, MauiMidiSaver>();
        builder.Services.AddSingleton<IThemeProvider, MauiThemeProvider>();
        builder.Services.AddSingleton<IDeviceDirectoryProvider, MauiDeviceDirectoryProvider>();
        builder.Services.AddBaroquenMelodyComponents();
        builder.Services.AddSingleton(AppInfo.Current);
        builder.Services.AddSingleton<IApplicationInfo, ApplicationInfo>();

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

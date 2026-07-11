using BaroquenMelody.App.Components.Extensions;
using BaroquenMelody.Infrastructure.Application;
using BaroquenMelody.Infrastructure.Devices;
using BaroquenMelody.Library.Midi;
using Bunit;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;

namespace BaroquenMelody.App.Components.Tests;

/// <summary>
///     A bUnit test context pre-wired like the MAUI host's container: the real component service graph
///     (MudBlazor, Fluxor, and the library's composition services) plus substitutes for the host-provided
///     services (theme, device info, MIDI launching and saving, application info).
/// </summary>
internal sealed class AppComponentsTestContext : Bunit.TestContext
{
    public AppComponentsTestContext(Action<IServiceCollection>? configureServices = null, bool renderPopoverProvider = true)
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        MockThemeProvider = Substitute.For<IThemeProvider>();
        MockThemeProvider.Theme.Returns(new MudTheme());

        MockPhysicalDeviceInfo = Substitute.For<IPhysicalDeviceInfo>();
        MockMidiLauncher = Substitute.For<IMidiLauncher>();
        MockMidiSaver = Substitute.For<IMidiSaver>();
        MockDeviceDirectoryProvider = Substitute.For<IDeviceDirectoryProvider>();
        MockApplicationInfo = Substitute.For<IApplicationInfo>();

        Services.AddBaroquenMelodyComponents();
        Services.AddSingleton(MockThemeProvider);
        Services.AddSingleton(MockPhysicalDeviceInfo);
        Services.AddSingleton(MockMidiLauncher);
        Services.AddSingleton(MockMidiSaver);
        Services.AddSingleton(MockDeviceDirectoryProvider);
        Services.AddSingleton(MockApplicationInfo);

        configureServices?.Invoke(Services);

        Services.GetRequiredService<IStore>().InitializeAsync().GetAwaiter().GetResult();

        // components that render their own provider (e.g. MainLayout) opt out, since MudBlazor
        // allows only one popover provider per renderer.
        PopoverProvider = renderPopoverProvider ? RenderComponent<MudPopoverProvider>() : null!;
    }

    /// <summary>
    ///     Hosts popover and tooltip content, which MudBlazor renders under the provider rather than
    ///     under the component that declares it. Unset when the context was created without one.
    /// </summary>
    public IRenderedComponent<MudPopoverProvider> PopoverProvider { get; }

    public IThemeProvider MockThemeProvider { get; }

    public IPhysicalDeviceInfo MockPhysicalDeviceInfo { get; }

    public IMidiLauncher MockMidiLauncher { get; }

    public IMidiSaver MockMidiSaver { get; }

    public IDeviceDirectoryProvider MockDeviceDirectoryProvider { get; }

    public IApplicationInfo MockApplicationInfo { get; }

    public IDispatcher Dispatcher => Services.GetRequiredService<IDispatcher>();

    public TState StateOf<TState>()
        where TState : class => Services.GetRequiredService<IState<TState>>().Value;
}

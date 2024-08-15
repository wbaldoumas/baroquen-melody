using BaroquenMelody;
using BaroquenMelody.Library;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;

var serviceProvider = new ServiceCollection()
    .AddLogging(loggingBuilder =>
    {
        loggingBuilder.SetMinimumLevel(LogLevel.Error);
        loggingBuilder.AddConsole();
    })
    .AddFluxor(fluxorOptions =>
    {
        fluxorOptions.WithLifetime(StoreLifetime.Singleton);
        fluxorOptions.ScanAssemblies(typeof(BaroquenMelodyComposerConfigurator).Assembly);
    })
    .AddSingleton<IBaroquenMelodyComposerConfigurator, BaroquenMelodyComposerConfigurator>()
    .AddScoped<App>()
    .BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

DispatchInitialState(scope.ServiceProvider.GetRequiredService<IDispatcher>());

var app = scope.ServiceProvider.GetRequiredService<App>();

var baroquenMelody = app.Run();
var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

baroquenMelody.MidiFile.Write($"test-{timestamp}.mid");

return;

void DispatchInitialState(IDispatcher dispatcher)
{
    dispatcher.Dispatch(new UpdateCompositionConfiguration(BaroquenScale.Parse("C Major"), Meter.ThreeFour, 25));

    dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.One, Notes.C5, Notes.E6, GeneralMidi2Program.AcousticGuitarNylon, true));
    dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.Two, Notes.G3, Notes.B4, GeneralMidi2Program.AcousticGuitarNylon, true));
    dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.Three, Notes.F3, Notes.A4, GeneralMidi2Program.AcousticGuitarNylon, true));

    // dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.Four, Notes.C2, Notes.E3, GeneralMidi2Program.AcousticGuitarNylon, true));
    foreach (var configuration in AggregateCompositionRuleConfiguration.Default.Configurations)
    {
        dispatcher.Dispatch(new UpdateCompositionRuleConfiguration(configuration.Rule, configuration.IsEnabled, configuration.Strictness));
    }

    foreach (var configuration in AggregateOrnamentationConfiguration.Default.Configurations)
    {
        dispatcher.Dispatch(new UpdateCompositionOrnamentationConfiguration(configuration.OrnamentationType, configuration.IsEnabled, configuration.Probability));
    }
}

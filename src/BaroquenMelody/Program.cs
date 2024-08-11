using BaroquenMelody;
using BaroquenMelody.Library;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;

var phrasingConfiguration = new PhrasingConfiguration(
    PhraseLengths: [1, 2, 3, 4, 5, 6, 7, 8],
    MaxPhraseRepetitions: 8,
    MinPhraseRepetitionPoolSize: 2,
    PhraseRepetitionProbability: 100
);

const Meter meter = Meter.FourFour;

var compositionConfiguration = new CompositionConfiguration(
    new HashSet<VoiceConfiguration>
    {
        new(Voice.Soprano, Notes.C5, Notes.E6, GeneralMidi2Program.AcousticGuitarNylon),
        new(Voice.Alto, Notes.G3, Notes.B4, GeneralMidi2Program.AcousticGuitarNylon),
        new(Voice.Tenor, Notes.F3, Notes.A4, GeneralMidi2Program.AcousticGuitarNylon),
        new(Voice.Bass, Notes.C2, Notes.E3, GeneralMidi2Program.AcousticGuitarNylon)
    },
    phrasingConfiguration,
    AggregateCompositionRuleConfiguration.Default,
    AggregateOrnamentationConfiguration.Default,
    BaroquenScale.Parse("C Major"),
    meter,
    meter.DefaultMusicalTimeSpan(),
    25
);

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

var app = scope.ServiceProvider.GetRequiredService<App>();
var baroquenMelody = app.Run(compositionConfiguration);
var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

baroquenMelody.MidiFile.Write($"test-{timestamp}.mid");

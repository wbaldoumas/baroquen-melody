using BaroquenMelody;
using BaroquenMelody.Infrastructure.FileSystem;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Midi;
using Melanchall.DryWetMidi.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;

var serviceProvider = new ServiceCollection()
    .AddLogging(loggingBuilder =>
    {
        loggingBuilder.SetMinimumLevel(LogLevel.Error);
        loggingBuilder.AddConsole();
    })
    .AddBaroquenMelody()
    .AddSingleton<IMidiSaver, StubMidiSaver>()
    .AddSingleton<IMidiLauncher, StubMidiLauncher>()
    .AddScoped<App>()
    .BuildServiceProvider();

var baroquenMelody = new BaroquenMelody.Library.MidiFileComposition(new MidiFile());

for (var i = 0; i < 10000; i++)
{
    try
    {
        using var scope = serviceProvider.CreateScope();

        var app = scope.ServiceProvider.GetRequiredService<App>();

        baroquenMelody = app.Run();

        Console.WriteLine($"Successfully composed composition {i}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to compose: {e.Message}");

        break;
    }
}

var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

baroquenMelody.MidiFile.Write($"test-{timestamp}.mid");

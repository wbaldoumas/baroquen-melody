using BaroquenMelody;
using BaroquenMelody.Library.Infrastructure.Extensions;
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
    .AddScoped<App>()
    .BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

var app = scope.ServiceProvider.GetRequiredService<App>();

var baroquenMelody = app.Run();

var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

baroquenMelody.MidiFile.Write($"test-{timestamp}.mid");

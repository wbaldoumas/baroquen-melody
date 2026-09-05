using BaroquenMelody.Library.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Extensions;

[TestFixture]
internal sealed class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddBaroquenMelody_ResolvesTheComposerConfigurator()
    {
        // arrange - the host supplies logging; everything else the configurator needs, including both of its
        // random providers, has to come from the registration itself
        var services = new ServiceCollection()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddBaroquenMelody();

        using var serviceProvider = services.BuildServiceProvider();

        // act
        var configurator = serviceProvider.GetRequiredService<IBaroquenMelodyComposerConfigurator>();

        // assert
        configurator.Should().BeOfType<BaroquenMelodyComposerConfigurator>();
    }
}

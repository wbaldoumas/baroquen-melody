using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors.Factories;

[TestFixture]
internal sealed class OrnamentationProcessorConfigurationFactoryTests
{
    private OrnamentationProcessorConfigurationFactory _ornamentationProcessorConfigurationFactory = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get();

        _ornamentationProcessorConfigurationFactory = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            new WeightedRandomBooleanGenerator(),
            compositionConfiguration,
            Substitute.For<ILogger>()
        );
    }

    [Test]
    public void OrnamentationProcessorConfigurationFactory_handles_all_ornamentation_types()
    {
        // arrange
        var excludedOrnamentationTypes = new HashSet<OrnamentationType>
        {
            OrnamentationType.None,
            OrnamentationType.Sustain,
            OrnamentationType.MidSustain,
            OrnamentationType.Rest
        }.ToFrozenSet();

        var ornamentationTypes = EnumUtils<OrnamentationType>.AsEnumerable()
            .Where(ornamentationType => !excludedOrnamentationTypes.Contains(ornamentationType))
            .ToList();

        foreach (var ornamentationType in ornamentationTypes.Select(ornamentationType => new OrnamentationConfiguration(ornamentationType, ConfigurationStatus.Enabled, 100)))
        {
            // act
            var act = () => _ornamentationProcessorConfigurationFactory.Create(ornamentationType);

            // assert
            act.Should().NotThrow();
        }
    }
}

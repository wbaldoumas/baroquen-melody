using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
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
            OrnamentationType.Rest,

            // suspension stamps are applied by the suspension applicator, not the configurable ornamentation
            // engine, so the factory has no processor to create for them
            OrnamentationType.Suspension,
            OrnamentationType.SuspensionResolution
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

    [Test]
    public void Create_ForAppoggiatura_GuardsTheLeaningToneWithDissonanceAndFreshApproachPolicies()
    {
        // act
        var configuration = _ornamentationProcessorConfigurationFactory
            .Create(new OrnamentationConfiguration(OrnamentationType.Appoggiatura, ConfigurationStatus.Enabled, 100))
            .Single();

        // assert - the leaning tone must genuinely clash and must not re-strike the voice's previous pitch
        configuration.InputPolicies.Should().ContainSingle(static policy => policy is LeaningToneIsDissonant);
        configuration.InputPolicies.Should().ContainSingle(static policy => policy is LeaningToneIsNotRestruck);
    }

    [Test]
    public void Create_ProducesConfigurationsLabeledWithTheRequestedOrnamentationType()
    {
        // arrange
        var excludedOrnamentationTypes = new HashSet<OrnamentationType>
        {
            OrnamentationType.None,
            OrnamentationType.Sustain,
            OrnamentationType.MidSustain,
            OrnamentationType.Rest,

            // suspension stamps are applied by the suspension applicator, not the configurable ornamentation
            // engine, so the factory has no processor to create for them
            OrnamentationType.Suspension,
            OrnamentationType.SuspensionResolution
        }.ToFrozenSet();

        var ornamentationTypes = EnumUtils<OrnamentationType>.AsEnumerable()
            .Where(ornamentationType => !excludedOrnamentationTypes.Contains(ornamentationType));

        foreach (var ornamentationType in ornamentationTypes)
        {
            // act
            var processorConfigurations = _ornamentationProcessorConfigurationFactory
                .Create(new OrnamentationConfiguration(ornamentationType, ConfigurationStatus.Enabled, 100))
                .ToList();

            // assert
            processorConfigurations.Should().NotBeEmpty();
            processorConfigurations.Should().OnlyContain(
                configuration => configuration.OrnamentationType == ornamentationType,
                "the '{0}' case must produce configurations labeled with that ornamentation type",
                ornamentationType
            );
        }
    }
}

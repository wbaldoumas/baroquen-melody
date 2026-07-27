using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
internal sealed class BaroquenMelodyComposerConfiguratorTests
{
    private ILogger<MidiFileComposition> _mockLogger = null!;

    private BaroquenMelodyComposerConfigurator _baroquenMelodyComposerConfigurator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = Substitute.For<ILogger<MidiFileComposition>>();
        _baroquenMelodyComposerConfigurator = new BaroquenMelodyComposerConfigurator(_mockLogger, Substitute.For<IDispatcher>(), new ThreadLocalRandomProvider(), new VoiceSpacingSatisfiabilityAnalyzer());
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Configure_returns_configured_MidiFileComposer_which_can_compose_a_MidiFileComposition(CompositionConfiguration compositionConfiguration)
    {
        // arrange
        var midiFileComposer = _baroquenMelodyComposerConfigurator.Configure(compositionConfiguration);

        // act
        var midiFileComposition = midiFileComposer.Compose(CancellationToken.None);

        // assert
        midiFileComposition.Should().NotBeNull();
        midiFileComposition.MidiFile.Should().NotBeNull();
    }

    private static IEnumerable<TestCaseData> TestCases =>
        from numberOfInstruments in Enumerable.Range(1, 3)
        from meter in EnumUtils<Meter>.AsEnumerable()
        from mode in EnumUtils<Mode>.AsEnumerable()
        select new TestCaseData(TestCompositionConfigurations.Get(numberOfInstruments, 10) with { Meter = meter, Mode = mode });

    [Test]
    public void Configure_WhenVoiceSpacingIsEnabledButUnsatisfiable_SkipsTheRuleAndComposes()
    {
        // arrange - without the dynamic disable, every candidate chord fails the voice spacing rule and the
        // composition dead-ends fatally once the theme composer's retries are exhausted
        _mockLogger.IsEnabled(LogLevel.Warning).Returns(true);

        var compositionConfiguration = GetConfigurationWithUnsatisfiableVoiceSpacing(AllRulesEnabled);

        var midiFileComposer = _baroquenMelodyComposerConfigurator.Configure(compositionConfiguration);

        // act
        var midiFileComposition = midiFileComposer.Compose(CancellationToken.None);

        // assert
        midiFileComposition.Should().NotBeNull();
        midiFileComposition.MidiFile.Should().NotBeNull();
    }

    [Test]
    public void Configure_WhenVoiceSpacingIsDisabledAndUnsatisfiable_Composes()
    {
        // arrange
        var ruleConfigurations = EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static rule => new CompositionRuleConfiguration(rule, rule == CompositionRule.EnforceVoiceSpacing ? ConfigurationStatus.Disabled : ConfigurationStatus.Enabled))
            .ToHashSet();

        var compositionConfiguration = GetConfigurationWithUnsatisfiableVoiceSpacing(new AggregateCompositionRuleConfiguration(ruleConfigurations));

        var midiFileComposer = _baroquenMelodyComposerConfigurator.Configure(compositionConfiguration);

        // act
        var midiFileComposition = midiFileComposer.Compose(CancellationToken.None);

        // assert
        midiFileComposition.Should().NotBeNull();
        midiFileComposition.MidiFile.Should().NotBeNull();
    }

    [Test]
    public void Configure_WhenVoiceSpacingIsAbsentFromTheRuleSetAndUnsatisfiable_Composes()
    {
        // arrange
        var compositionConfiguration = GetConfigurationWithUnsatisfiableVoiceSpacing(new AggregateCompositionRuleConfiguration(new HashSet<CompositionRuleConfiguration>()));

        var midiFileComposer = _baroquenMelodyComposerConfigurator.Configure(compositionConfiguration);

        // act
        var midiFileComposition = midiFileComposer.Compose(CancellationToken.None);

        // assert
        midiFileComposition.Should().NotBeNull();
        midiFileComposition.MidiFile.Should().NotBeNull();
    }

    private static AggregateCompositionRuleConfiguration AllRulesEnabled => new(
        EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static rule => new CompositionRuleConfiguration(rule))
            .ToHashSet()
    );

    /// <summary>
    ///     A three-voice configuration identical to the standard test configuration except that voice One is raised
    ///     so high that it can never be within an octave of voice Two, making the voice spacing rule unsatisfiable.
    /// </summary>
    private static CompositionConfiguration GetConfigurationWithUnsatisfiableVoiceSpacing(AggregateCompositionRuleConfiguration aggregateCompositionRuleConfiguration) => new(
        new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, Notes.C6, Notes.C7, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Two, Notes.G2, Notes.G4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Three, Notes.C2, Notes.C3, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        },
        PhrasingConfiguration.Default,
        aggregateCompositionRuleConfiguration,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 10
    );
}

using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
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

// Every test builds its own configurator: NUnit shares one fixture instance across the parallel cases, so fixture
// state set in a [SetUp] would race under ParallelScope.All.
[TestFixture]
[Parallelizable(ParallelScope.All)]
internal sealed class BaroquenMelodyComposerConfiguratorTests
{
    [Test]
    [TestCaseSource(nameof(RepresentativeTestCases))]
    public void Configure_returns_configured_MidiFileComposer_which_can_compose_a_MidiFileComposition(CompositionConfiguration compositionConfiguration) => AssertComposes(compositionConfiguration);

    [Test]
    [Category(TestCategories.WholeComposition)]
    [TestCaseSource(nameof(RemainingTestCases))]
    public void Configure_composes_in_every_other_mode_and_meter(CompositionConfiguration compositionConfiguration) => AssertComposes(compositionConfiguration);

    [Test]
    public void BuildRelativeConfiguration_ForwardsEveryParameterExceptTheKeyCenter()
    {
        // arrange - the relative configuration is built through the full positional constructor, where a
        // silently-defaulting trailing parameter is invisible at the call site and the ground form never
        // reads some of the forwarded values, so no behavioral test could catch a drop. This guard builds
        // a source whose every parameter holds a non-default value and demands each one survive into the
        // relative key; a constructor parameter missing from the sample map fails loudly, so a future
        // addition must register here before its forwarding can be forgotten there.
        var sampleValues = new Dictionary<string, object?>
        {
            [nameof(CompositionConfiguration.InstrumentConfigurations)] = new HashSet<InstrumentConfiguration> { InstrumentConfiguration.DefaultConfigurations[Instrument.One], InstrumentConfiguration.DefaultConfigurations[Instrument.Two] },
            [nameof(CompositionConfiguration.PhrasingConfiguration)] = PhrasingConfiguration.Default,
            [nameof(CompositionConfiguration.AggregateCompositionRuleConfiguration)] = AggregateCompositionRuleConfiguration.Default,
            [nameof(CompositionConfiguration.AggregateOrnamentationConfiguration)] = AggregateOrnamentationConfiguration.Default,
            [nameof(CompositionConfiguration.Tonic)] = NoteName.A,
            [nameof(CompositionConfiguration.Mode)] = Mode.Aeolian,
            [nameof(CompositionConfiguration.Meter)] = Meter.ThreeFour,
            [nameof(CompositionConfiguration.DefaultNoteTimeSpan)] = MusicalTimeSpan.Quarter,
            [nameof(CompositionConfiguration.MinimumMeasures)] = 37,
            [nameof(CompositionConfiguration.CompositionContextSize)] = 5,
            [nameof(CompositionConfiguration.Tempo)] = 133,
            [nameof(CompositionConfiguration.ShuffleOrnamentationProcessors)] = false,
            [nameof(CompositionConfiguration.MaxLookAheadDepth)] = 2,
            [nameof(CompositionConfiguration.AggregateScoringRuleConfiguration)] = AggregateScoringRuleConfiguration.Default,
            [nameof(CompositionConfiguration.MotifDevelopmentConfiguration)] = MotifDevelopmentConfiguration.Default,
            [nameof(CompositionConfiguration.HarmonicRhythmConfiguration)] = HarmonicRhythmConfiguration.Default,
            [nameof(CompositionConfiguration.SuspensionConfiguration)] = SuspensionConfiguration.Default,
            [nameof(CompositionConfiguration.TonicizationConfiguration)] = TonicizationConfiguration.Default,
            [nameof(CompositionConfiguration.GroundBassConfiguration)] = new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca, Modulate: false, Divisions: false),
            [nameof(CompositionConfiguration.VoiceRhythmConfiguration)] = new VoiceRhythmConfiguration(Enabled: false),
            [nameof(CompositionConfiguration.TextureConfiguration)] = new TextureConfiguration(TextureType.Walking)
        };

        var constructor = typeof(CompositionConfiguration).GetConstructors().Single();
        var parameters = constructor.GetParameters();

        foreach (var parameter in parameters)
        {
            sampleValues.Should().ContainKey(parameter.Name!, "every constructor parameter needs a non-default sample so the relative forwarding stays guarded as the configuration grows");
        }

        var sourceConfiguration = (CompositionConfiguration)constructor.Invoke([.. parameters.Select(parameter => sampleValues[parameter.Name!])]);

        // act
        var relativeConfiguration = BaroquenMelodyComposerConfigurator.BuildRelativeConfiguration(sourceConfiguration);

        // assert - only the key center moves, to the relative major
        relativeConfiguration.Mode.Should().Be(Mode.Ionian, "an Aeolian home modulates to its relative major");
        relativeConfiguration.Tonic.Should().Be(NoteName.C, "A minor's relative major is C");

        foreach (var parameter in parameters)
        {
            if (parameter.Name is nameof(CompositionConfiguration.Tonic) or nameof(CompositionConfiguration.Mode))
            {
                continue;
            }

            var property = typeof(CompositionConfiguration).GetProperty(parameter.Name!);

            property.Should().NotBeNull();
            property!.GetValue(relativeConfiguration).Should().Be(property.GetValue(sourceConfiguration), "the relative configuration must forward {0}", parameter.Name);
        }
    }

    [Test]
    public void Configure_WhenVoiceSpacingIsEnabledButUnsatisfiable_SkipsTheRuleAndComposes()
    {
        // arrange - without the dynamic disable, every candidate chord fails the voice spacing rule and the
        // composition dead-ends fatally once the theme composer's retries are exhausted
        var mockLogger = Substitute.For<ILogger<MidiFileComposition>>();

        mockLogger.IsEnabled(LogLevel.Warning).Returns(true);

        var compositionConfiguration = GetConfigurationWithUnsatisfiableVoiceSpacing(AllRulesEnabled);

        var midiFileComposer = CreateConfigurator(mockLogger).Configure(compositionConfiguration);

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

        var midiFileComposer = CreateConfigurator().Configure(compositionConfiguration);

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

        var midiFileComposer = CreateConfigurator().Configure(compositionConfiguration);

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

    /// <summary>
    ///     The (mode, meter) pairs that stay in Stryker's universe: every meter, the Ionian/Aeolian pair whose gates
    ///     the modulation and tonicization passes lift, and three of the other modes for their scale colours
    ///     (Phrygian's lowered second, Lydian's raised fourth, Dorian's raised sixth). Mixolydian and Locrian have no
    ///     representative: the Library has no mode-specific branch, so no path loses its only coverage, and the
    ///     measured loss already counts their absence. The other fifteen pairs run in CI's unit leg only: measured
    ///     over the whole Library, leaving them out of Stryker's universe lost 5 of 1,657 kills and cut this matrix's
    ///     share of a covering mutant's test run from about 46 s to about 13 s.
    /// </summary>
    private static readonly (Mode Mode, Meter Meter)[] RepresentativePairs =
    [
        (Mode.Ionian, Meter.FourFour),
        (Mode.Phrygian, Meter.FourFour),
        (Mode.Lydian, Meter.FourFour),
        (Mode.Ionian, Meter.ThreeFour),
        (Mode.Aeolian, Meter.ThreeFour),
        (Mode.Dorian, Meter.FiveEight)
    ];

    private static IEnumerable<TestCaseData> RepresentativeTestCases => TestCases(static pair => RepresentativePairs.Contains(pair));

    private static IEnumerable<TestCaseData> RemainingTestCases => TestCases(static pair => !RepresentativePairs.Contains(pair));

    private static IEnumerable<TestCaseData> TestCases(Func<(Mode Mode, Meter Meter), bool> isSelected) =>
        from numberOfInstruments in Enumerable.Range(1, 3)
        from meter in EnumUtils<Meter>.AsEnumerable()
        from mode in EnumUtils<Mode>.AsEnumerable()
        where isSelected((mode, meter))
        select new TestCaseData(TestCompositionConfigurations.Get(numberOfInstruments, 10) with { Meter = meter, Mode = mode })
            .SetArgDisplayNames($"{numberOfInstruments}-voice {mode} {meter}");

    private static BaroquenMelodyComposerConfigurator CreateConfigurator(ILogger<MidiFileComposition>? logger = null) => new(
        logger ?? Substitute.For<ILogger<MidiFileComposition>>(),
        Substitute.For<IDispatcher>(),
        new ThreadLocalRandomProvider(),
        new ThreadLocalRandomProvider(),
        new VoiceSpacingSatisfiabilityAnalyzer());

    private static void AssertComposes(CompositionConfiguration compositionConfiguration)
    {
        // arrange
        var midiFileComposer = CreateConfigurator().Configure(compositionConfiguration);

        // act
        var midiFileComposition = midiFileComposer.Compose(CancellationToken.None);

        // assert
        midiFileComposition.Should().NotBeNull();
        midiFileComposition.MidiFile.Should().NotBeNull();
    }
}

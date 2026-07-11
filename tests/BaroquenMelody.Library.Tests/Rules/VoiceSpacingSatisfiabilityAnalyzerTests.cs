using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Rules;

[TestFixture]
internal sealed class VoiceSpacingSatisfiabilityAnalyzerTests
{
    private static readonly BaroquenScale _cMajorScale = new(NoteName.C, Mode.Ionian);

    private VoiceSpacingSatisfiabilityAnalyzer _voiceSpacingSatisfiabilityAnalyzer = null!;

    [SetUp]
    public void SetUp() => _voiceSpacingSatisfiabilityAnalyzer = new VoiceSpacingSatisfiabilityAnalyzer();

    [Test]
    public void IsSatisfiable_WithFewerThanThreeVoices_IsTriviallySatisfiable()
    {
        // arrange - two voices with registers that could never be within an octave of each other, but
        // voice spacing only constrains adjacent pairs of upper voices, which requires at least three voices
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C7, Notes.C8),
            Voice(Instrument.Two, Notes.C1, Notes.C2)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeTrue();
    }

    [Test]
    public void IsSatisfiable_WithBridgeableAdjacentRanges_ReturnsTrue()
    {
        // arrange
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C4, Notes.C6),
            Voice(Instrument.Two, Notes.G2, Notes.G4),
            Voice(Instrument.Three, Notes.C2, Notes.C3)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeTrue();
    }

    [Test]
    public void IsSatisfiable_WhenTheTopPairOfVoicesCanNeverMeet_ReturnsFalse()
    {
        // arrange - the highest playable note of voice Two (C3) is two octaves below the lowest playable note of voice One (C6)
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C6, Notes.C7),
            Voice(Instrument.Two, Notes.C2, Notes.C3),
            Voice(Instrument.Three, Notes.C1, Notes.C2)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeFalse();
    }

    [Test]
    public void IsSatisfiable_WhenAdjacentVoicesCanMeetExactlyAnOctaveApart_ReturnsTrue()
    {
        // arrange - the closest playable notes of voices One and Two (C5 and C4) are exactly an octave apart, which the rule allows
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C5, Notes.C6),
            Voice(Instrument.Two, Notes.C3, Notes.C4),
            Voice(Instrument.Three, Notes.C1, Notes.C2)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeTrue();
    }

    [Test]
    public void IsSatisfiable_WhenAdjacentVoicesAreAlwaysMoreThanAnOctaveApart_ReturnsFalse()
    {
        // arrange - the closest playable notes of voices One and Two (C5 and B3) are thirteen semitones apart, one past the octave
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C5, Notes.C6),
            Voice(Instrument.Two, Notes.B2, Notes.B3),
            Voice(Instrument.Three, Notes.C1, Notes.C2)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeFalse();
    }

    [Test]
    public void IsSatisfiable_WhenAMiddleVoiceBridgesTheChain_ReturnsTrue()
    {
        // arrange - voice Two's feasible notes (G3..G5 restricted to within an octave of voice One: C4..G5)
        // can still reach voice Three's C4 at exactly an octave from its lowest playable note C3
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C5, Notes.C6),
            Voice(Instrument.Two, Notes.G3, Notes.G5),
            Voice(Instrument.Three, Notes.C3, Notes.C4),
            Voice(Instrument.Four, Notes.C1, Notes.C2)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeTrue();
    }

    [Test]
    public void IsSatisfiable_UsesPropagatedFeasibleNotesRatherThanRawRanges_ReturnsFalse()
    {
        // arrange - voice Two's raw range (C3..C6) overlaps voice Three's range, but the notes it can actually
        // use (within an octave of voice One's playable notes: C5..C6) cannot reach voice Three; a naive
        // pairwise range comparison would wrongly report this configuration as satisfiable
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.C6, Notes.C7),
            Voice(Instrument.Two, Notes.C3, Notes.C6),
            Voice(Instrument.Three, Notes.C2, Notes.C3),
            Voice(Instrument.Four, Notes.C1, Notes.C2)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeFalse();
    }

    [Test]
    public void IsSatisfiable_OrdersVoicesByRegisterRegardlessOfInputOrder_ReturnsFalse()
    {
        // arrange - in input order the adjacent pair (Two, Three) is bridgeable, but ordered by register
        // (as the voice spacing rule orders voices) the constrained pair is (One, Two), which can never meet
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.Two, Notes.C2, Notes.C3),
            Voice(Instrument.Three, Notes.C1, Notes.C2),
            Voice(Instrument.One, Notes.C6, Notes.C7)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeFalse();
    }

    [Test]
    public void IsSatisfiable_WhenAConstrainedVoiceHasNoPlayableScaleNotes_ReturnsFalse()
    {
        // arrange - voice One's range contains only F#6, which is not in C Major, so no chord can be voiced at all
        var instrumentConfigurations = new[]
        {
            Voice(Instrument.One, Notes.FSharp6, Notes.FSharp6),
            Voice(Instrument.Two, Notes.C4, Notes.C5),
            Voice(Instrument.Three, Notes.C2, Notes.C3)
        };

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(instrumentConfigurations, _cMajorScale);

        // assert
        isSatisfiable.Should().BeFalse();
    }

    [Test]
    public void IsSatisfiable_ForACompositionConfigurationWithBridgeableRanges_ReturnsTrue()
    {
        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(TestCompositionConfigurations.Get(4));

        // assert
        isSatisfiable.Should().BeTrue();
    }

    [Test]
    public void IsSatisfiable_ForACompositionConfigurationWithUnbridgeableRanges_ReturnsFalse()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                Voice(Instrument.One, Notes.C6, Notes.C7),
                Voice(Instrument.Two, Notes.C2, Notes.C3),
                Voice(Instrument.Three, Notes.C1, Notes.C2)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 10
        );

        // act
        var isSatisfiable = _voiceSpacingSatisfiabilityAnalyzer.IsSatisfiable(compositionConfiguration);

        // assert
        isSatisfiable.Should().BeFalse();
    }

    private static InstrumentConfiguration Voice(Instrument instrument, Note minNote, Note maxNote) => new(
        instrument,
        minNote,
        maxNote,
        InstrumentConfiguration.DefaultMinVelocity,
        InstrumentConfiguration.DefaultMaxVelocity,
        GeneralMidi2Program.AcousticGrandPiano,
        ConfigurationStatus.Enabled
    );
}

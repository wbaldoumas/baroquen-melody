using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

/// <summary>
///     The body walk's voice-rhythm integration: where the pinned search fires, how the v1 whole-chord hold
///     outranks it, and which emitted notes the ledger records.
/// </summary>
[TestFixture]
internal sealed class ComposerVoiceRhythmTests
{
    private IChordComposer _mockChordComposer = null!;

    private IVoiceRhythmScheduler _mockVoiceRhythmScheduler = null!;

    private IVoiceRhythmLedger _mockVoiceRhythmLedger = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private List<BaroquenChord> _composedChords = null!;

    private Composer _composer = null!;

    [SetUp]
    public void SetUp()
    {
        _compositionConfiguration = TestCompositionConfigurations.Get(numberOfInstruments: 3, compositionLength: 4);
        _composedChords = [];

        _mockChordComposer = Substitute.For<IChordComposer>();
        _mockVoiceRhythmScheduler = Substitute.For<IVoiceRhythmScheduler>();
        _mockVoiceRhythmLedger = Substitute.For<IVoiceRhythmLedger>();

        _mockChordComposer
            .Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(_ => TrackComposedChord());

        _mockChordComposer
            .Compose(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenNote>())
            .Returns(_ => TrackComposedChord());

        var mockThemeComposer = Substitute.For<IThemeComposer>();

        mockThemeComposer.Compose(Arg.Any<CancellationToken>()).Returns(_ => new BaroquenTheme([CreateMeasure()], [CreateMeasure()]));

        var mockEndingComposer = Substitute.For<IEndingComposer>();

        mockEndingComposer
            .Compose(Arg.Any<Composition>(), Arg.Any<BaroquenTheme>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Composition>(0));

        _composer = new Composer(
            Substitute.For<ICompositionDecorator>(),
            Substitute.For<ICompositionPhraser>(),
            _mockChordComposer,
            new HarmonicRhythmScheduler(_compositionConfiguration),
            _mockVoiceRhythmScheduler,
            _mockVoiceRhythmLedger,
            Substitute.For<ISuspensionApplicator>(),
            Substitute.For<ITonicizationApplicator>(),
            mockThemeComposer,
            mockEndingComposer,
            Substitute.For<IDynamicsApplicator>(),
            Substitute.For<IDispatcher>(),
            _compositionConfiguration);
    }

    [Test]
    public void Compose_AtAScheduledPinSite_PinsTheHeldVoicesPreviousNote()
    {
        // arrange - the scheduler pins Instrument.One at the fresh interior beat of measure one; with the
        // default harmonic rhythm, that measure's walk is: beat 0 fresh, beat 1 held duplicate, beat 2
        // pinned, beat 3 held duplicate. The pin must carry the pitch the voice already sounds.
        _mockVoiceRhythmScheduler
            .TryGetPinnedInstrument(1, 2, out Arg.Any<Instrument>())
            .Returns(callInfo =>
            {
                callInfo[2] = Instrument.One;

                return true;
            });

        // act
        _composer.Compose(CancellationToken.None);

        // assert - the walk's fifth free chord opens measure one, and its pitch rides the beat-one duplicate
        // into the pin at beat two
        var measureOneOpeningChord = _composedChords[4];

        _mockChordComposer
            .Received(1)
            .Compose(
                Arg.Any<IReadOnlyList<BaroquenChord>>(),
                Arg.Is<BaroquenNote>(pinnedNote => pinnedNote.Instrument == Instrument.One && pinnedNote.Raw == measureOneOpeningChord[Instrument.One].Raw));
    }

    [Test]
    public void Compose_WithNothingScheduled_NeverTouchesThePinnedPath()
    {
        // act
        _composer.Compose(CancellationToken.None);

        // assert
        _mockChordComposer.DidNotReceive().Compose(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenNote>());
    }

    [Test]
    public void Compose_AtABeatTheHarmonicRhythmHolds_NeverConsultsThePinScheduler()
    {
        // arrange - beat one of measure one is a whole-chord hold under the default grid, so the walk
        // duplicates the chord and moves on before any voice-rhythm decision exists to make
        _mockVoiceRhythmScheduler
            .TryGetPinnedInstrument(1, 1, out Arg.Any<Instrument>())
            .Returns(callInfo =>
            {
                callInfo[2] = Instrument.One;

                return true;
            });

        // act
        _composer.Compose(CancellationToken.None);

        // assert
        _mockVoiceRhythmScheduler.DidNotReceive().TryGetPinnedInstrument(1, 1, out Arg.Any<Instrument>());
        _mockChordComposer.DidNotReceive().Compose(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenNote>());
    }

    [Test]
    public void Compose_RecordsTheHeldAndFloridVoicesNotesForEveryBeatOfTheirMeasures()
    {
        // arrange - measure one carries a held voice and a florid voice; every one of its four beats
        // (the attack, the grid's duplicates, and the pinned beat) must be recorded for suppression
        // and boosting downstream
        _mockVoiceRhythmScheduler
            .TryGetHeldInstrument(1, out Arg.Any<Instrument>())
            .Returns(callInfo =>
            {
                callInfo[1] = Instrument.One;

                return true;
            });

        _mockVoiceRhythmScheduler
            .TryGetFloridInstrument(1, out Arg.Any<Instrument>())
            .Returns(callInfo =>
            {
                callInfo[1] = Instrument.Two;

                return true;
            });

        // act
        _composer.Compose(CancellationToken.None);

        // assert
        _mockVoiceRhythmLedger.Received(1).Clear();
        _mockVoiceRhythmLedger.Received(4).RecordHeldNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.One));
        _mockVoiceRhythmLedger.Received(4).RecordFloridNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Two));
        _mockVoiceRhythmLedger.DidNotReceive().RecordHeldNote(Arg.Is<BaroquenNote>(note => note.Instrument != Instrument.One));
        _mockVoiceRhythmLedger.DidNotReceive().RecordFloridNote(Arg.Is<BaroquenNote>(note => note.Instrument != Instrument.Two));
    }

    private BaroquenChord TrackComposedChord()
    {
        // Every tracked chord carries a distinct top-voice pitch, so the pin-source assertion actually
        // discriminates WHICH chord the pin came from rather than matching any chord ever emitted.
        var composedChord = CreateChord(_composedChords.Count);

        _composedChords.Add(composedChord);

        return composedChord;
    }

    private static Measure CreateMeasure() => new([new Beat(CreateChord(0))], Meter.FourFour);

    private static BaroquenChord CreateChord(int chordIndex)
    {
        Melanchall.DryWetMidi.MusicTheory.Note[] topVoicePitches = [Notes.C5, Notes.D5, Notes.E5, Notes.F5, Notes.G5, Notes.A5, Notes.B5];

        return new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, topVoicePitches[chordIndex % topVoicePitches.Length], MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half)
        ]);
    }
}

using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
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

    private ICompositionPhraser _mockCompositionPhraser = null!;

    private IVoiceRhythmScheduler _mockVoiceRhythmScheduler = null!;

    private IVoiceRhythmLedger _mockVoiceRhythmLedger = null!;

    private IThemeComposer _mockThemeComposer = null!;

    private IEndingComposer _mockEndingComposer = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private List<BaroquenChord> _composedChords = null!;

    private Composer _composer = null!;

    [SetUp]
    public void SetUp()
    {
        _compositionConfiguration = TestCompositionConfigurations.Get(numberOfInstruments: 3, compositionLength: 4);
        _composedChords = [];

        _mockChordComposer = Substitute.For<IChordComposer>();
        _mockCompositionPhraser = Substitute.For<ICompositionPhraser>();
        _mockVoiceRhythmScheduler = Substitute.For<IVoiceRhythmScheduler>();
        _mockVoiceRhythmLedger = Substitute.For<IVoiceRhythmLedger>();

        _mockChordComposer
            .Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(_ => TrackComposedChord());

        _mockChordComposer
            .Compose(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenNote>())
            .Returns(_ => TrackComposedChord());

        _mockThemeComposer = Substitute.For<IThemeComposer>();

        _mockThemeComposer.Compose(Arg.Any<CancellationToken>()).Returns(_ => new BaroquenTheme([CreateMeasure()], [CreateMeasure()]));

        _mockEndingComposer = Substitute.For<IEndingComposer>();

        _mockEndingComposer
            .Compose(Arg.Any<Composition>(), Arg.Any<BaroquenTheme>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Composition>(0));

        _composer = CreateComposer(_compositionConfiguration);
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

    [Test]
    public void Compose_WithTextureRolesScheduled_RoutesEachRoleToItsStore()
    {
        // arrange - the static assignment: melody rides the florid store, pads ride the held store, and only
        // the figuration voice carries the new mark; every beat of every body measure records
        ScheduleTextureRole(Instrument.One, TextureRole.Melody);
        ScheduleTextureRole(Instrument.Two, TextureRole.Pad);
        ScheduleTextureRole(Instrument.Three, TextureRole.Figuration);

        // act
        _composer.Compose(CancellationToken.None);

        // assert - four body measures of four beats each, recorded once by the walk and once more,
        // idempotently on the real reference-keyed stores, by the post-phrasing restatement pass
        _mockVoiceRhythmLedger.Received(32).RecordFloridNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.One));
        _mockVoiceRhythmLedger.Received(32).RecordHeldNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Two));
        _mockVoiceRhythmLedger.Received(32).RecordTextureFigurationNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Three));
        _mockVoiceRhythmLedger.DidNotReceive().RecordFloridNote(Arg.Is<BaroquenNote>(note => note.Instrument != Instrument.One));
        _mockVoiceRhythmLedger.DidNotReceive().RecordHeldNote(Arg.Is<BaroquenNote>(note => note.Instrument != Instrument.Two));
        _mockVoiceRhythmLedger.DidNotReceive().RecordTextureFigurationNote(Arg.Is<BaroquenNote>(note => note.Instrument != Instrument.Three));
    }

    [Test]
    public void Compose_WithATextureActiveAndANarrowWindow_ClampsTheAccompanimentOffsetIntoTheWindow()
    {
        // arrange - the prominence pass runs after dynamics (mocked inert here, so every note sits at the
        // domain default of 75) and offsets only the notes the ledger knows as pads or figuration; the
        // test ranges clamp 75 - 8 = 67 to the 60 ceiling's floor-bounded window, so this test decides
        // ONLY the clamp - the offset's magnitude is decided by the unclamped-window pin below
        ArrangeActiveTexture();

        _mockVoiceRhythmLedger.IsHeldNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Two)).Returns(true);
        _mockVoiceRhythmLedger.IsTextureFigurationNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Three)).Returns(true);

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert
        var notes = composition.Measures.SelectMany(static measure => measure.Beats).SelectMany(static beat => beat.Chord.Notes).ToList();

        notes.Where(static note => note.Instrument == Instrument.One).Should().OnlyContain(static note => (int)note.Velocity == 75, "the melody keeps its full voice");
        notes.Where(static note => note.Instrument != Instrument.One).Should().OnlyContain(static note => (int)note.Velocity == 60, "accompaniment notes offset by -8, clamped into the instrument's velocity window");
    }

    [Test]
    public void Compose_WithATextureActiveAndAnUnclampedWindow_OffsetsAccompanimentByExactlyTheNamedConstant()
    {
        // arrange - a velocity window wide enough that the clamp can never mask the magnitude: with the
        // dynamics pass inert every note sits at the domain default of 75, so the accompaniment - and
        // every mirrored sub-note - must land at exactly 75 - 8, and a drifted offset constant fails
        // here instead of vanishing into the clamp
        var composer = CreateComposer(BuildUnclampedVelocityConfiguration());

        _mockChordComposer
            .Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(static _ => CreateChordWithAFiguredBass());

        ArrangeActiveTexture();

        _mockVoiceRhythmLedger.IsHeldNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Two)).Returns(true);
        _mockVoiceRhythmLedger.IsTextureFigurationNote(Arg.Is<BaroquenNote>(note => note.Instrument == Instrument.Three)).Returns(true);

        // act
        var composition = composer.Compose(CancellationToken.None);

        // assert
        var notes = composition.Measures.SelectMany(static measure => measure.Beats).SelectMany(static beat => beat.Chord.Notes).ToList();
        var bassSubNotes = notes.Where(static note => note.Instrument == Instrument.Three).SelectMany(static note => note.Ornamentations).ToList();

        notes.Where(static note => note.Instrument == Instrument.One).Should().OnlyContain(static note => (int)note.Velocity == 75, "the melody keeps its full voice");
        notes.Where(static note => note.Instrument != Instrument.One).Should().OnlyContain(static note => (int)note.Velocity == 67, "the accompaniment offsets by exactly the named constant when the window cannot clamp");
        bassSubNotes.Should().NotBeEmpty("the figured bass carries sub-notes for the mirroring pin");
        bassSubNotes.Should().OnlyContain(static note => (int)note.Velocity == 67, "sub-notes mirror their principal's exact offset");
    }

    [Test]
    public void Compose_WithATextureActive_ReclothesRestatementCopiesButNeverWalkedOriginals()
    {
        // arrange - every walked chord carries a figure on every voice; the phraser inserts one deep copy
        // of a body measure (reference-unequal, so unrecorded); the ledger answers recorded for everything
        // except the copy's notes. The copy's accompaniment must shed its copied-in figures and record,
        // the copy's melody must keep its figures, and the walked originals must never be touched.
        ScheduleTextureRole(Instrument.One, TextureRole.Melody);
        ScheduleTextureRole(Instrument.Two, TextureRole.Pad);
        ScheduleTextureRole(Instrument.Three, TextureRole.Figuration);

        _mockChordComposer
            .Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(static _ => CreateFullyFiguredChord());

        // The domain overrides note equality by value (equal-pitch repeats are legitimately equal), so
        // copy membership must be decided by reference identity, exactly as the ledger decides it.
        var copiedNotes = new List<BaroquenNote>();

        bool IsCopied(BaroquenNote note) => copiedNotes.Any(copiedNote => ReferenceEquals(copiedNote, note));

        _mockCompositionPhraser
            .When(static phraser => phraser.AttemptPhraseRepetition(Arg.Any<List<Measure>>()))
            .Do(callInfo =>
            {
                if (copiedNotes.Count > 0)
                {
                    return;
                }

                var measures = callInfo.Arg<List<Measure>>();
                var restatement = new Measure(measures[^1]);

                copiedNotes.AddRange(restatement.Beats.SelectMany(static beat => beat.Chord.Notes));
                measures.Add(restatement);
            });

        _mockVoiceRhythmLedger.IsHeldNote(Arg.Any<BaroquenNote>()).Returns(callInfo => !IsCopied(callInfo.Arg<BaroquenNote>()));
        _mockVoiceRhythmLedger.IsTextureFigurationNote(Arg.Any<BaroquenNote>()).Returns(callInfo => !IsCopied(callInfo.Arg<BaroquenNote>()));

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert
        copiedNotes.Should().NotBeEmpty("the phraser inserted a restatement");

        var copiedAccompanimentNotes = copiedNotes.Where(static note => note.Instrument != Instrument.One).ToList();
        var walkedBodyNotes = composition.Measures
            .Skip(1)
            .SelectMany(static measure => measure.Beats)
            .SelectMany(static beat => beat.Chord.Notes)
            .Where(note => !IsCopied(note))
            .ToList();

        copiedAccompanimentNotes.Should().NotBeEmpty("the restatement carries accompaniment voices");
        copiedAccompanimentNotes.Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.None && note.Ornamentations.Count == 0, "an accompaniment copy sheds its copied-in figures for the texture to re-clothe");
        copiedNotes.Where(static note => note.Instrument == Instrument.One).Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.Mordent && note.Ornamentations.Count == 1, "the melody keeps its copied figures - thematic recall is its privilege");

        walkedBodyNotes.Should().NotBeEmpty("the walked body surrounds the restatement");
        walkedBodyNotes.Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.Mordent, "walked originals are already recorded, so re-clothing never strips them");

        _mockVoiceRhythmLedger.Received().RecordFloridNote(Arg.Is<BaroquenNote>(note => copiedNotes.Any(copiedNote => ReferenceEquals(copiedNote, note)) && note.Instrument == Instrument.One));
        _mockVoiceRhythmLedger.Received().RecordHeldNote(Arg.Is<BaroquenNote>(note => copiedNotes.Any(copiedNote => ReferenceEquals(copiedNote, note)) && note.Instrument == Instrument.Two));
        _mockVoiceRhythmLedger.Received().RecordTextureFigurationNote(Arg.Is<BaroquenNote>(note => copiedNotes.Any(copiedNote => ReferenceEquals(copiedNote, note)) && note.Instrument == Instrument.Three));
    }

    [Test]
    public void Compose_WithNoTextureActive_NeverTouchesAnyVelocity()
    {
        // arrange - outside a texture the held store carries the rotation's roles, which must never be
        // offset; the pass gates on the scheduler's texture answer before reading anything
        _mockVoiceRhythmLedger.IsHeldNote(Arg.Any<BaroquenNote>()).Returns(true);

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert
        composition.Measures
            .SelectMany(static measure => measure.Beats)
            .SelectMany(static beat => beat.Chord.Notes)
            .Should().OnlyContain(static note => (int)note.Velocity == 75, "no texture means no prominence offsets, whatever the ledger holds");
    }

    private Composer CreateComposer(CompositionConfiguration compositionConfiguration) => new(
        Substitute.For<ICompositionDecorator>(),
        _mockCompositionPhraser,
        _mockChordComposer,
        new HarmonicRhythmScheduler(compositionConfiguration),
        _mockVoiceRhythmScheduler,
        _mockVoiceRhythmLedger,
        Substitute.For<ISuspensionApplicator>(),
        Substitute.For<ITonicizationApplicator>(),
        _mockThemeComposer,
        _mockEndingComposer,
        Substitute.For<IDynamicsApplicator>(),
        Substitute.For<IDispatcher>(),
        compositionConfiguration);

    private void ArrangeActiveTexture()
    {
        _mockVoiceRhythmScheduler
            .TryGetTextureDecorationOrder(out Arg.Any<IReadOnlyList<Instrument>>())
            .Returns(static callInfo =>
            {
                callInfo[0] = (IReadOnlyList<Instrument>)[Instrument.One, Instrument.Two, Instrument.Three];

                return true;
            });
    }

    // A velocity window wide enough that the prominence offset can never clamp. Constructed fresh - the
    // derived instrument properties are property initializers a with-clone would leave stale.
    private static CompositionConfiguration BuildUnclampedVelocityConfiguration() => new(
        new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, Notes.C4, Notes.C6, new SevenBitNumber(20), new SevenBitNumber(110), GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Two, Notes.G2, Notes.G4, new SevenBitNumber(20), new SevenBitNumber(110), GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Three, Notes.C2, Notes.C3, new SevenBitNumber(20), new SevenBitNumber(110), GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        },
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 4);

    private static BaroquenChord CreateChordWithAFiguredBass()
    {
        var chord = CreateChord(0);
        var bassNote = chord[Instrument.Three];

        bassNote.OrnamentationType = OrnamentationType.Mordent;
        bassNote.Ornamentations.Add(new BaroquenNote(Instrument.Three, Notes.D3, MusicalTimeSpan.Eighth));

        return chord;
    }

    private static BaroquenChord CreateFullyFiguredChord()
    {
        var chord = CreateChord(0);

        foreach (var note in chord.Notes)
        {
            note.OrnamentationType = OrnamentationType.Mordent;
            note.Ornamentations.Add(new BaroquenNote(note.Instrument, Notes.D3, MusicalTimeSpan.Eighth));
        }

        return chord;
    }

    private void ScheduleTextureRole(Instrument instrument, TextureRole textureRole)
    {
        _mockVoiceRhythmScheduler
            .TryGetTextureRole(instrument, out Arg.Any<TextureRole>())
            .Returns(callInfo =>
            {
                callInfo[1] = textureRole;

                return true;
            });
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

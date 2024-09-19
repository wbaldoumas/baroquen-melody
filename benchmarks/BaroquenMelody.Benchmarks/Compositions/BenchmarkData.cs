using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Benchmarks.Compositions;

internal static class BenchmarkData
{
    public static readonly BaroquenScale BaroquenScale = new(NoteName.C, Mode.Ionian);

    public static readonly CompositionConfiguration CompositionConfiguration = new(
        new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, Notes.C0, Notes.C6),
            new(Instrument.Two, Notes.C0, Notes.C6),
            new(Instrument.Three, Notes.C0, Notes.C6),
            new(Instrument.Four, Notes.C0, Notes.C6)
        },
        new PhrasingConfiguration(
            PhraseLengths: [1, 2, 4, 8],
            MaxPhraseRepetitions: 8,
            MinPhraseRepetitionPoolSize: 4,
            PhraseRepetitionProbability: 100
        ),
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.D,
        Mode.Dorian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        25
    );

    public static readonly BaroquenChord CMajor = new([
        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord EMinor = new([
        new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Four, Notes.B1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord FMajor = new([
        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Four, Notes.F1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord GMajor = new([
        new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord DMinor = new([
        new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Instrument.Four, Notes.D1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly ChordChoice ChordChoice = new(
        [
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 3),
            new NoteChoice(Instrument.Two, NoteMotion.Descending, 3),
            new NoteChoice(Instrument.Three, NoteMotion.Ascending, 3),
            new NoteChoice(Instrument.Four, NoteMotion.Oblique, 0)
        ]
    );
}

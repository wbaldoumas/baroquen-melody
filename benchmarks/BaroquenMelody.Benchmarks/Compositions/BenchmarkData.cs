using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Benchmarks.Compositions;

internal static class BenchmarkData
{
    public static readonly CompositionConfiguration CompositionConfiguration = new(
        new HashSet<VoiceConfiguration>
        {
            new(Voice.One, Notes.C0, Notes.C6),
            new(Voice.Two, Notes.C0, Notes.C6),
            new(Voice.Three, Notes.C0, Notes.C6),
            new(Voice.Four, Notes.C0, Notes.C6)
        },
        new PhrasingConfiguration(
            PhraseLengths: [1, 2, 4, 8],
            MaxPhraseRepetitions: 8,
            MinPhraseRepetitionPoolSize: 4,
            PhraseRepetitionProbability: 100
        ),
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        BaroquenScale.Parse("D Dorian"),
        Meter.FourFour,
        MusicalTimeSpan.Half,
        25
    );

    public static readonly BaroquenChord CMajor = new([
        new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Three, Notes.G2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Four, Notes.C1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord EMinor = new([
        new BaroquenNote(Voice.One, Notes.B4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Three, Notes.G2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Four, Notes.B1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord FMajor = new([
        new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Two, Notes.F3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Three, Notes.A2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Four, Notes.F1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord GMajor = new([
        new BaroquenNote(Voice.One, Notes.D4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Two, Notes.G3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Three, Notes.B2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Four, Notes.G1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord DMinor = new([
        new BaroquenNote(Voice.One, Notes.D4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Two, Notes.F3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Three, Notes.A2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Four, Notes.D1, MusicalTimeSpan.Quarter)
    ]);
}

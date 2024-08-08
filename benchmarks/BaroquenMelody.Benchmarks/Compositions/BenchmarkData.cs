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
            new(Voice.Soprano, Notes.C0, Notes.C6),
            new(Voice.Alto, Notes.C0, Notes.C6),
            new(Voice.Tenor, Notes.C0, Notes.C6),
            new(Voice.Bass, Notes.C0, Notes.C6)
        },
        new PhrasingConfiguration(
            PhraseLengths: [1, 2, 4, 8],
            MaxPhraseRepetitions: 8,
            MinPhraseRepetitionPoolSize: 4,
            PhraseRepetitionProbability: 100
        ),
        AggregateCompositionRuleConfiguration.Default,
        BaroquenScale.Parse("D Dorian"),
        Meter.FourFour,
        MusicalTimeSpan.Half,
        25
    );

    public static readonly BaroquenChord CMajor = new([
        new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Bass, Notes.C1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord EMinor = new([
        new BaroquenNote(Voice.Soprano, Notes.B4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Bass, Notes.B1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord FMajor = new([
        new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Tenor, Notes.A2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Bass, Notes.F1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord GMajor = new([
        new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Alto, Notes.G3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Tenor, Notes.B2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Bass, Notes.G1, MusicalTimeSpan.Quarter)
    ]);

    public static readonly BaroquenChord DMinor = new([
        new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Tenor, Notes.A2, MusicalTimeSpan.Quarter),
        new BaroquenNote(Voice.Bass, Notes.D1, MusicalTimeSpan.Quarter)
    ]);
}

using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Tests.TestData;

internal static class Configurations
{
    public static CompositionConfiguration CompositionConfiguration => new(
        new HashSet<VoiceConfiguration>
        {
            new(Voice.Soprano, Notes.C4, Notes.C5),
            new(Voice.Alto, Notes.C3, Notes.C4),
            new(Voice.Tenor, Notes.C2, Notes.C3),
            new(Voice.Bass, Notes.C1, Notes.C2)
        },
        BaroquenScale.Parse("C Major"),
        Meter.FourFour,
        CompositionLength: 100
    );
}

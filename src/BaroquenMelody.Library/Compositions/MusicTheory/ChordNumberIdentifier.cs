using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

internal sealed class ChordNumberIdentifier(CompositionConfiguration compositionConfiguration) : IChordNumberIdentifier
{
    public ChordNumber IdentifyChordNumber(BaroquenChord chord) => chord.Notes switch
    {
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.I.Contains(note.Raw.NoteName)) => ChordNumber.I,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.II.Contains(note.Raw.NoteName)) => ChordNumber.II,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.III.Contains(note.Raw.NoteName)) => ChordNumber.III,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.IV.Contains(note.Raw.NoteName)) => ChordNumber.IV,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.V.Contains(note.Raw.NoteName)) => ChordNumber.V,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.VI.Contains(note.Raw.NoteName)) => ChordNumber.VI,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.VII.Contains(note.Raw.NoteName)) => ChordNumber.VII,
        _ => ChordNumber.Unknown
    };
}

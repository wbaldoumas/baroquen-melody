using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="IChordNumberIdentifier"/>
internal sealed class ChordNumberIdentifier(CompositionConfiguration compositionConfiguration) : IChordNumberIdentifier
{
    public ChordNumber IdentifyChordNumber(BaroquenChord chord) => chord.Notes switch
    {
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.I.Contains(note.NoteName)) => ChordNumber.I,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.II.Contains(note.NoteName)) => ChordNumber.II,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.III.Contains(note.NoteName)) => ChordNumber.III,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.IV.Contains(note.NoteName)) => ChordNumber.IV,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.V.Contains(note.NoteName)) => ChordNumber.V,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.VI.Contains(note.NoteName)) => ChordNumber.VI,
        var notes when notes.TrueForAll(note => compositionConfiguration.Scale.VII.Contains(note.NoteName)) => ChordNumber.VII,
        _ => ChordNumber.Unknown
    };
}

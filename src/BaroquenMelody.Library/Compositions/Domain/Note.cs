using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Domain;

internal sealed record Note(Voice Voice, Melanchall.DryWetMidi.MusicTheory.Note Raw);

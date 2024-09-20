using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionConfiguration(NoteName RootNote, Mode Mode, Meter Meter, int CompositionLength = 25, int Tempo = 120);

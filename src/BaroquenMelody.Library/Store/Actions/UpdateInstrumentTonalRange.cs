using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateInstrumentTonalRange(Instrument Instrument, Note LowestPitchNote, Note HighestPitchNote);

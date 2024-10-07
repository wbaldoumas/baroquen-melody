using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateMidiInstrument(Instrument Instrument, GeneralMidi2Program MidiInstrument);

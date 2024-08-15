using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateVoiceConfiguration(Voice Voice, Note MinNote, Note MaxNote, GeneralMidi2Program Instrument, bool IsEnabled);

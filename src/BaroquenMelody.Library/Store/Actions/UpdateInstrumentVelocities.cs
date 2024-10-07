using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateInstrumentVelocities(Instrument Instrument, SevenBitNumber MinVelocity, SevenBitNumber MaxVelocity);

using BaroquenMelody.Library.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Exceptions;

[ExcludeFromCodeCoverage(Justification = "Exception with no logic.")]
internal sealed class CouldNotFindStartingNoteForInstrumentException(Instrument instrument) : Exception($"Could not find starting note for instrument {instrument}.");

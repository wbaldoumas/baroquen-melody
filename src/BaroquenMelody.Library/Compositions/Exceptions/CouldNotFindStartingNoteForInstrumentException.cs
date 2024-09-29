using BaroquenMelody.Library.Compositions.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Exceptions;

[ExcludeFromCodeCoverage(Justification = "Exception with no logic.")]
internal sealed class CouldNotFindStartingNoteForInstrumentException(Instrument instrument) : Exception($"Could not find starting note for instrument {instrument}.");

using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Exceptions;

[ExcludeFromCodeCoverage(Justification = "Exception with no logic.")]
internal sealed class NoValidChordChoicesAvailableException() : Exception("No valid chord choices are available.");

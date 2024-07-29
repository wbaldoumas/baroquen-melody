using BaroquenMelody.Library.Compositions.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Infrastructure.Exceptions;

[ExcludeFromCodeCoverage(Justification = "Exception with no logic.")]
internal sealed class CouldNotFindStartingNoteForVoiceException(Voice voice) : Exception($"Could not find starting note for voice {voice}.");

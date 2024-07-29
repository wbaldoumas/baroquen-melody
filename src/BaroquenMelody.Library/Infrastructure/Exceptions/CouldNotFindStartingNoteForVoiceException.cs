using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Infrastructure.Exceptions;

internal sealed class CouldNotFindStartingNoteForVoiceException(Voice voice) : Exception($"Could not find starting note for voice {voice}.");

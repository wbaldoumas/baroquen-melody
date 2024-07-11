using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

internal interface INoteTransposer
{
    IEnumerable<BaroquenNote> TransposeToVoice(IEnumerable<BaroquenNote> notesToTranspose, Voice oldVoice, Voice newVoice);
}

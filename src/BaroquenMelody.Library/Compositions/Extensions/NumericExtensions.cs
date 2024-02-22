using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Extensions;

internal static class NumericExtensions
{
    public static Note ToNote(this int noteNumber) => ((byte)noteNumber).ToNote();

    public static Note ToNote(this byte noteNumber) => Note.Get(new SevenBitNumber(noteNumber));
}

using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.MusicTheory.Extensions;

internal static class MusicalTimeSpanExtensions
{
    public static int DivideBy(this MusicalTimeSpan dividend, MusicalTimeSpan divisor) => Convert.ToInt32(dividend.Divide(divisor));
}

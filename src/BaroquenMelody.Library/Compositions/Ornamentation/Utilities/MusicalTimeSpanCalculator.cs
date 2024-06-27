using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

internal sealed class MusicalTimeSpanCalculator : IMusicalTimeSpanCalculator
{
    public MusicalTimeSpan CalculatePrimaryNoteTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.SixteenthNoteRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth.Dotted(1),
        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, null)
    };

    public MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.SixteenthNoteRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, null)
    };
}

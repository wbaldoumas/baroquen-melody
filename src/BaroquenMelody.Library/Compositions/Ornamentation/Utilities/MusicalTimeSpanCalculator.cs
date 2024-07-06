using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

/// <inheritdoc cref="IMusicalTimeSpanCalculator"/>
internal sealed class MusicalTimeSpanCalculator : IMusicalTimeSpanCalculator
{
    private static MusicalTimeSpan Zero => new();

    public MusicalTimeSpan CalculatePrimaryNoteTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.None when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.SixteenthNoteRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth.Dotted(1),
        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.AlternateTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Sustain when meter == Meter.FourFour => MusicalTimeSpan.Half,
        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.ThirtySecond,
        OrnamentationType.DelayedThirtySecondNoteRun when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Rest => Zero,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };

    public MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.None => throw new NotSupportedException($"{nameof(OrnamentationType.None)} cannot be applied to an ornamentation."),
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.SixteenthNoteRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.AlternateTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Sustain => throw new NotSupportedException($"{nameof(OrnamentationType.Sustain)} cannot be applied to an ornamentation."),
        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.ThirtySecond,
        OrnamentationType.DelayedThirtySecondNoteRun when meter == Meter.FourFour => MusicalTimeSpan.ThirtySecond,
        OrnamentationType.Rest => Zero,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };
}

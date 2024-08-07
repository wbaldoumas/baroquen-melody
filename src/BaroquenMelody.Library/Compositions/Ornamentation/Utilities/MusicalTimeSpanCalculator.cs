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
        OrnamentationType.None when meter == Meter.FourFour => MusicalTimeSpan.Half,
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Run when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.AlternateTurn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Sustain when meter == Meter.FourFour => MusicalTimeSpan.Whole,
        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedRun when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DecorateInterval when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DoubleRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Pedal when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Mordent when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.RepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedRepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.NeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedNeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.MidSustain => Zero,
        OrnamentationType.Rest => Zero,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };

    public MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter, int ornamentationStep = 1) => ornamentationType switch
    {
        OrnamentationType.None => throw new NotSupportedException($"{nameof(OrnamentationType.None)} cannot be applied to an ornamentation."),
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Run when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.AlternateTurn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Sustain => throw new NotSupportedException($"{nameof(OrnamentationType.Sustain)} cannot be applied to an ornamentation."),
        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DecorateInterval when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DoubleRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Pedal when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Mordent when meter == Meter.FourFour && ornamentationStep == 1 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.FourFour && ornamentationStep == 2 => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.RepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedRepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.NeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedNeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.MidSustain => Zero,
        OrnamentationType.Rest => Zero,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };
}

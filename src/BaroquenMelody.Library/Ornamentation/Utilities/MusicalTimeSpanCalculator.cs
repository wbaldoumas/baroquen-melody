using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Ornamentation.Utilities;

/// <inheritdoc cref="IMusicalTimeSpanCalculator"/>
internal sealed class MusicalTimeSpanCalculator : IMusicalTimeSpanCalculator
{
    private static MusicalTimeSpan Zero => new();

    public MusicalTimeSpan CalculatePrimaryNoteTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.None when meter == Meter.FourFour => MusicalTimeSpan.Half,
        OrnamentationType.None when meter == Meter.ThreeFour => MusicalTimeSpan.Half.Dotted(1),
        OrnamentationType.None when meter == Meter.FiveEight => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.PassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half,
        OrnamentationType.PassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.Pickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Pickup when meter == Meter.ThreeFour => MusicalTimeSpan.Half,
        OrnamentationType.Pickup when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedPickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedPickup when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPickup when meter == Meter.FiveEight => MusicalTimeSpan.Half,

        OrnamentationType.DoublePickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DoublePickup when meter == Meter.ThreeFour => MusicalTimeSpan.Half,
        OrnamentationType.DoublePickup when meter == Meter.FiveEight => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DelayedDoublePickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedDoublePickup when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedDoublePickup when meter == Meter.FiveEight => MusicalTimeSpan.Half,

        OrnamentationType.Run when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Run when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.Run when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedPassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Half,

        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Turn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.Turn when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.InvertedTurn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.InvertedTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.InvertedTurn when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.Sustain when meter == Meter.FourFour => MusicalTimeSpan.Whole,
        OrnamentationType.Sustain when meter == Meter.ThreeFour => MusicalTimeSpan.Half.Dotted(1) + MusicalTimeSpan.Half.Dotted(1),
        OrnamentationType.Sustain when meter == Meter.FiveEight => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth + MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleTurn when meter == Meter.FiveEight => MusicalTimeSpan.Eighth.Dotted(1),

        OrnamentationType.DoubleInvertedTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleInvertedTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleInvertedTurn when meter == Meter.FiveEight => MusicalTimeSpan.Eighth.Dotted(1),

        OrnamentationType.DelayedRun when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedRun when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedRun when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DoublePassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half,
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DecorateInterval when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateInterval when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DecorateInterval when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DoubleRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleRun when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleRun when meter == Meter.FiveEight => MusicalTimeSpan.Eighth.Dotted(1),

        OrnamentationType.Pedal when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Pedal when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.Pedal when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.Mordent when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.FiveEight => MusicalTimeSpan.Sixteenth,

        OrnamentationType.RepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.RepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Half,
        OrnamentationType.RepeatedNote when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedRepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedRepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedRepeatedNote when meter == Meter.FiveEight => MusicalTimeSpan.Half,

        OrnamentationType.NeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.NeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half,
        OrnamentationType.NeighborTone when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedNeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedNeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedNeighborTone when meter == Meter.FiveEight => MusicalTimeSpan.Half,

        OrnamentationType.DecorateThird when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DecorateThird when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DecorateThird when meter == Meter.FiveEight => MusicalTimeSpan.Quarter,

        OrnamentationType.MidSustain => Zero,
        OrnamentationType.Rest => Zero,

        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };

    public MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter, int ornamentationStep = 1) => ornamentationType switch
    {
        OrnamentationType.None => throw new NotSupportedException($"{nameof(OrnamentationType.None)} cannot be applied to an ornamentation."),

        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.PassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.PassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.Pickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Pickup when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Pickup when meter == Meter.FiveEight => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DelayedPickup when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPickup when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPickup when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.DoublePickup when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DoublePickup when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DoublePickup when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.DelayedDoublePickup when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedDoublePickup when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedDoublePickup when meter == Meter.FiveEight => MusicalTimeSpan.Sixteenth,

        OrnamentationType.Run when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Run when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Run when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Turn when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Turn when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.InvertedTurn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.InvertedTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.InvertedTurn when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.Sustain => throw new NotSupportedException($"{nameof(OrnamentationType.Sustain)} cannot be applied to an ornamentation."),

        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleTurn when meter == Meter.FiveEight => MusicalTimeSpan.Sixteenth,

        OrnamentationType.DoubleInvertedTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleInvertedTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleInvertedTurn when meter == Meter.FiveEight => MusicalTimeSpan.Sixteenth,

        OrnamentationType.DelayedRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedRun when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedRun when meter == Meter.FiveEight => MusicalTimeSpan.Sixteenth.Dotted(1),

        OrnamentationType.DoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DoublePassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Eighth.Dotted(1),

        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.DecorateInterval when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateInterval when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateInterval when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.DoubleRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleRun when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleRun when meter == Meter.FiveEight => MusicalTimeSpan.Sixteenth,

        OrnamentationType.Pedal when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Pedal when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Pedal when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.Mordent when meter == Meter.FourFour && ornamentationStep == 0 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.ThreeFour && ornamentationStep == 0 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.FiveEight && ornamentationStep == 0 => MusicalTimeSpan.Sixteenth,

        OrnamentationType.Mordent when meter == Meter.FourFour && ornamentationStep == 1 => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.Mordent when meter == Meter.ThreeFour && ornamentationStep == 1 => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
        OrnamentationType.Mordent when meter == Meter.FiveEight && ornamentationStep == 1 => MusicalTimeSpan.Half,

        OrnamentationType.RepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.RepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.RepeatedNote when meter == Meter.FiveEight => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DelayedRepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedRepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedRepeatedNote when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.NeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.NeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,
        OrnamentationType.NeighborTone when meter == Meter.FiveEight => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DelayedNeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedNeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedNeighborTone when meter == Meter.FiveEight => MusicalTimeSpan.Eighth,

        OrnamentationType.DecorateThird when meter == Meter.FourFour && ornamentationStep == 0 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DecorateThird when meter == Meter.FourFour && ornamentationStep == 1 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DecorateThird when meter == Meter.FourFour && ornamentationStep == 2 => MusicalTimeSpan.Eighth,

        OrnamentationType.DecorateThird when meter == Meter.ThreeFour && ornamentationStep == 0 => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateThird when meter == Meter.ThreeFour && ornamentationStep == 1 => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateThird when meter == Meter.ThreeFour && ornamentationStep == 2 => MusicalTimeSpan.Quarter,

        OrnamentationType.DecorateThird when meter == Meter.FiveEight && ornamentationStep == 0 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DecorateThird when meter == Meter.FiveEight && ornamentationStep == 1 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DecorateThird when meter == Meter.FiveEight && ornamentationStep == 2 => MusicalTimeSpan.Quarter,

        OrnamentationType.MidSustain => Zero,
        OrnamentationType.Rest => Zero,

        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };
}

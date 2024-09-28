using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

/// <inheritdoc cref="IMusicalTimeSpanCalculator"/>
internal sealed class MusicalTimeSpanCalculator : IMusicalTimeSpanCalculator
{
    private static MusicalTimeSpan Zero => new();

#pragma warning disable MA0051 // Method is too long
    public MusicalTimeSpan CalculatePrimaryNoteTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.None when meter == Meter.FourFour => MusicalTimeSpan.Half,
        OrnamentationType.None when meter == Meter.ThreeFour => MusicalTimeSpan.Half.Dotted(1),

        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.PassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half,

        OrnamentationType.Pickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Pickup when meter == Meter.ThreeFour => MusicalTimeSpan.Half,

        OrnamentationType.DelayedPickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedPickup when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.Run when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Run when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedPassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Turn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.InvertedTurn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.InvertedTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.Sustain when meter == Meter.FourFour => MusicalTimeSpan.Whole,
        OrnamentationType.Sustain when meter == Meter.ThreeFour => MusicalTimeSpan.Half.Dotted(1) + MusicalTimeSpan.Half.Dotted(1),

        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth,

        OrnamentationType.DelayedRun when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DelayedRun when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.DoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.DoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half,

        OrnamentationType.DecorateInterval when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateInterval when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.DoubleRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleRun when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth,

        OrnamentationType.Pedal when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Pedal when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter.Dotted(1),

        OrnamentationType.Mordent when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,

        OrnamentationType.RepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.RepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Half,

        OrnamentationType.DelayedRepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedRepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.NeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.NeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half,

        OrnamentationType.DelayedNeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.DelayedNeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.MidSustain => Zero,
        OrnamentationType.Rest => Zero,

        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };

#pragma warning disable MA0051 // Method is too long
    public MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter, int ornamentationStep = 1) => ornamentationType switch
    {
        OrnamentationType.None => throw new NotSupportedException($"{nameof(OrnamentationType.None)} cannot be applied to an ornamentation."),

        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.PassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.Pickup when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.Pickup when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedPickup when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPickup when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.Run when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Run when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.DelayedPassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedPassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.Turn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Turn when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.InvertedTurn when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.InvertedTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.Sustain => throw new NotSupportedException($"{nameof(OrnamentationType.Sustain)} cannot be applied to an ornamentation."),

        OrnamentationType.DoubleTurn when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleTurn when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,

        OrnamentationType.DelayedRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedRun when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.DoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedDoublePassingTone when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DelayedDoublePassingTone when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.DecorateInterval when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DecorateInterval when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.DoubleRun when meter == Meter.FourFour => MusicalTimeSpan.Sixteenth,
        OrnamentationType.DoubleRun when meter == Meter.ThreeFour => MusicalTimeSpan.Sixteenth,

        OrnamentationType.Pedal when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.Pedal when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.Mordent when meter == Meter.FourFour && ornamentationStep == 1 => MusicalTimeSpan.Sixteenth,
        OrnamentationType.Mordent when meter == Meter.ThreeFour && ornamentationStep == 1 => MusicalTimeSpan.Sixteenth,

        OrnamentationType.Mordent when meter == Meter.FourFour && ornamentationStep == 2 => MusicalTimeSpan.Quarter.Dotted(1),
        OrnamentationType.Mordent when meter == Meter.ThreeFour && ornamentationStep == 2 => MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,

        OrnamentationType.RepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.RepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedRepeatedNote when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedRepeatedNote when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.NeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Quarter,
        OrnamentationType.NeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Quarter,

        OrnamentationType.DelayedNeighborTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        OrnamentationType.DelayedNeighborTone when meter == Meter.ThreeFour => MusicalTimeSpan.Eighth,

        OrnamentationType.MidSustain => Zero,
        OrnamentationType.Rest => Zero,

        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, $"Invalid {nameof(OrnamentationType)}")
    };
}

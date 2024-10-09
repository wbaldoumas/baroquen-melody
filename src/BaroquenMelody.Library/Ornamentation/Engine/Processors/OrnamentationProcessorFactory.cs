using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class OrnamentationProcessorFactory(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator
) : IOrnamentationProcessorFactory
{
    public IProcessor<OrnamentationItem> Create(OrnamentationType ornamentationType, CompositionConfiguration compositionConfiguration, int interval = 0) => ornamentationType switch
    {
        OrnamentationType.PassingTone => new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.Run => new RunProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.DelayedPassingTone => new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.Turn => new TurnProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.InvertedTurn => new InvertedTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.DelayedRun => new DelayedRunProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.DoubleTurn => new DoubleTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.DoubleInvertedTurn => new DoubleInvertedTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.DoublePassingTone => new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.DelayedDoublePassingTone => new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.DecorateInterval => new DecorateIntervalProcessor(musicalTimeSpanCalculator, compositionConfiguration, interval),
        OrnamentationType.DoubleRun => new DoubleRunProcessor(musicalTimeSpanCalculator, compositionConfiguration),
        OrnamentationType.Pedal => new PedalProcessor(musicalTimeSpanCalculator, compositionConfiguration, interval),
        OrnamentationType.Mordent => new MordentProcessor(musicalTimeSpanCalculator, weightedRandomBooleanGenerator, compositionConfiguration),
        OrnamentationType.RepeatedNote => new RepeatedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.DelayedRepeatedNote => new RepeatedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.NeighborTone => new NeighborToneProcessor(musicalTimeSpanCalculator, weightedRandomBooleanGenerator, ornamentationType, compositionConfiguration),
        OrnamentationType.DelayedNeighborTone => new NeighborToneProcessor(musicalTimeSpanCalculator, weightedRandomBooleanGenerator, ornamentationType, compositionConfiguration),
        OrnamentationType.Pickup => new PickupProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.DelayedPickup => new PickupProcessor(musicalTimeSpanCalculator, compositionConfiguration, ornamentationType),
        OrnamentationType.Rest => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not supported."),
        OrnamentationType.None => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not supported."),
        OrnamentationType.Sustain => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not supported."),
        OrnamentationType.MidSustain => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not supported."),
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, "Ornamentation type not supported.")
    };
}

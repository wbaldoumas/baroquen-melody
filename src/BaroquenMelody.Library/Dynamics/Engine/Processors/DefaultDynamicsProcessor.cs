using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Extensions;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Dynamics.Engine.Utilities;

namespace BaroquenMelody.Library.Dynamics.Engine.Processors;

internal sealed class DefaultDynamicsProcessor(
    IVelocityCalculator velocityCalculator,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    CompositionConfiguration configuration
) : IProcessor<DynamicsApplicationItem>
{
    public void Process(DynamicsApplicationItem item)
    {
        var instrumentConfiguration = configuration.InstrumentConfigurationsByInstrument[item.Instrument];
        var precedingVelocity = velocityCalculator.GetPrecedingVelocity(item.PrecedingBeats, item.Instrument);
        var canIncreaseVelocity = precedingVelocity < instrumentConfiguration.MaxVelocity;
        var canDecreaseVelocity = precedingVelocity > instrumentConfiguration.MinVelocity;

        var newVelocity = (canIncreaseVelocity, canDecreaseVelocity) switch
        {
            (canIncreaseVelocity: false, canDecreaseVelocity: false) => precedingVelocity,
            (canIncreaseVelocity: true, canDecreaseVelocity: false) => precedingVelocity.Increment(),
            (canIncreaseVelocity: false, canDecreaseVelocity: true) => precedingVelocity.Decrement(),
            (canIncreaseVelocity: true, canDecreaseVelocity: true) => weightedRandomBooleanGenerator.IsTrue() ? precedingVelocity.Increment() : precedingVelocity.Decrement()
        };

        var currentNote = item.CurrentBeat[item.Instrument];

        currentNote.Velocity = newVelocity;

        foreach (var ornamentation in currentNote.Ornamentations)
        {
            ornamentation.Velocity = newVelocity;
        }

        item.HasProcessedCurrentBeat = true;
    }
}

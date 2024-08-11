using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;

internal sealed class ThreeFourOrnamentationCleaningEngineBuilder(
    IProcessor<OrnamentationCleaningItem> halfQuarterOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> eighthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> halfEighthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> sixteenthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> delayedRunEighthOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> doublePassingToneQuarterOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> doublePassingToneOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> halfQuarterEighthOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> quarterQuarterEighthOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> doublePassingToneDelayedRunOrnamentationCleaner
) : IOrnamentationCleaningEngineBuilder
{
    public IPolicyEngine<OrnamentationCleaningItem> Build() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildHalfQuarterOrnamentationCleanerEngine(),
            BuildEighthNoteOrnamentationCleanerEngine(),
            BuildHalfEighthNoteOrnamentationCleanerEngine(),
            BuildSixteenthNoteOrnamentationCleanerEngine(),
            BuildDelayedRunEighthOrnamentationCleanerEngine(),
            BuildDoublePassingToneQuarterOrnamentationCleanerEngine(),
            BuildDoublePassingToneOrnamentationCleanerEngine(),
            BuildHalfQuarterEighthOrnamentationCleanerEngine(),
            BuildQuarterQuarterEighthOrnamentationCleanerEngine(),
            BuildDoublePassingToneDelayedRunOrnamentationCleanerEngine()
        )
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildHalfQuarterOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.PassingTone)
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.NeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.PassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.Pickup))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.NeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.NeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.NeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.RepeatedNote))
        )
        .WithProcessors(halfQuarterOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildEighthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DecorateInterval, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pedal, OrnamentationType.Pedal))
        )
        .WithProcessors(eighthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildHalfEighthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone)
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedRepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPickup, OrnamentationType.DelayedPickup))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPickup, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPickup, OrnamentationType.DelayedNeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPickup, OrnamentationType.DelayedRepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedRepeatedNote))
        )
        .WithProcessors(halfEighthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.DoubleRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.DoubleTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn))
        )
        .WithProcessors(sixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildDelayedRunEighthOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DelayedRun, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRun, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRun, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRun, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRun, OrnamentationType.Pedal))
        )
        .WithProcessors(delayedRunEighthOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildDoublePassingToneQuarterOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone)
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Pickup))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.NeighborTone))
        )
        .WithProcessors(doublePassingToneQuarterOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildDoublePassingToneOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone)
        )
        .WithProcessors(doublePassingToneOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildHalfQuarterEighthOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.Run))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.Pickup, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Run))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.Run))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.Run))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.Pedal))
        )
        .WithProcessors(halfQuarterEighthOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildQuarterQuarterEighthOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal))
        )
        .WithProcessors(quarterQuarterEighthOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IProcessor<OrnamentationCleaningItem> BuildDoublePassingToneDelayedRunOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DelayedRun)
        )
        .WithProcessors(doublePassingToneDelayedRunOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();
}

using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;

internal sealed class OrnamentationCleaningEngineBuilder(
    IProcessor<OrnamentationCleaningItem> passingToneOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> eighthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> quarterEighthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> turnAlternateTurnOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> sixteenthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> sixteenthEighthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> mordentEighthNoteOrnamentationCleaner)
{
    public IPolicyEngine<OrnamentationCleaningItem> Build() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildPassingToneOrnamentationCleanerEngine(),
            BuildEighthNoteOrnamentationCleanerEngine(),
            BuildPassingToneEighthNoteOrnamentationCleanerEngine(),
            BuildTurnAlternateTurnOrnamentationCleanerEngine(),
            BuildSixteenthNoteOrnamentationCleanerEngine(),
            BuildSixteenthEighthNoteOrnamentationCleanerEngine(),
            BuildMordentEighthNoteOrnamentationCleanerEngine()
        )
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildPassingToneOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.PassingTone)
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.PassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.NeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.PassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote))
        )
        .WithProcessors(passingToneOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildEighthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.Turn))
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

    private IPolicyEngine<OrnamentationCleaningItem> BuildPassingToneEighthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.Run, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.RepeatedNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedNote, OrnamentationType.DecorateInterval))
        )
        .WithProcessors(quarterEighthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildTurnAlternateTurnOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.AlternateTurn))
        .WithProcessors(turnAlternateTurnOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.DoubleRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.DoubleTurn))
        )
        .WithProcessors(sixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildSixteenthEighthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleRun, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Run))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Pedal))
        )
        .WithProcessors(sixteenthEighthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildMordentEighthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Run)
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Pedal))
        )
        .WithProcessors(mordentEighthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();
}

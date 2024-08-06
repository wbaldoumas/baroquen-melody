using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;

internal sealed class OrnamentationCleaningEngineBuilder(
    IProcessor<OrnamentationCleaningItem> passingToneOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> sixteenthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> eighthSixteenthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> turnAlternateTurnOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> thirtySecondNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> thirtySecondSixteenthNoteOrnamentationCleaner,
    IProcessor<OrnamentationCleaningItem> mordentSixteenthNoteOrnamentationCleaner)
{
    public IPolicyEngine<OrnamentationCleaningItem> Build() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildPassingToneOrnamentationCleanerEngine(),
            BuildSixteenthNoteOrnamentationCleanerEngine(),
            BuildPassingToneSixteenthNoteOrnamentationCleanerEngine(),
            BuildTurnAlternateTurnOrnamentationCleanerEngine(),
            BuildThirtySecondNoteOrnamentationCleanerEngine(),
            BuildThirtySecondSixteenthNoteOrnamentationCleanerEngine(),
            BuildMordentSixteenthNoteOrnamentationCleanerEngine()
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
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.PassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.NeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.PassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.RepeatedEighthNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedPassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.DelayedNeighborTone, OrnamentationType.RepeatedDottedEighthSixteenth))
        )
        .WithProcessors(passingToneOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.Pedal))
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
        .WithProcessors(sixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildPassingToneSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.RepeatedEighthNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.RepeatedEighthNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.DoublePassingTone))
                .Or(new HasTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.RepeatedEighthNote))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.DecorateInterval))
        )
        .WithProcessors(eighthSixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildTurnAlternateTurnOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(new HasTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.AlternateTurn))
        .WithProcessors(turnAlternateTurnOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildThirtySecondNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.ThirtySecondNoteRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DoubleTurn))
        )
        .WithProcessors(thirtySecondNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildThirtySecondSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.SixteenthNoteRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Pedal))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.SixteenthNoteRun))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Pedal))
        )
        .WithProcessors(thirtySecondSixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildMordentSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.SixteenthNoteRun)
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Turn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.AlternateTurn))
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.DecorateInterval))
                .Or(new HasTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Pedal))
        )
        .WithProcessors(mordentSixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();
}

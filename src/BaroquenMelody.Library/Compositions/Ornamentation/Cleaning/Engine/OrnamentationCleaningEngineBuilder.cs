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
            new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.PassingTone)
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.PassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.DoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedPassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedDoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.NeighborTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.DelayedPassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.DelayedDoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.NeighborTone, OrnamentationType.RepeatedDottedEighthSixteenth))
        )
        .WithProcessors(passingToneOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun)
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.AlternateTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.Turn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.AlternateTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DecorateInterval, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Pedal, OrnamentationType.Pedal))
        )
        .WithProcessors(sixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildPassingToneSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun)
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Turn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.PassingTone, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.DoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.SixteenthNoteRun, OrnamentationType.RepeatedEighthNote))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.DoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.RepeatedEighthNote))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.DoublePassingTone))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.AlternateTurn, OrnamentationType.RepeatedEighthNote))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.RepeatedEighthNote, OrnamentationType.DecorateInterval))
        )
        .WithProcessors(eighthSixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildTurnAlternateTurnOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(new NotesContainTargetOrnamentations(OrnamentationType.Turn, OrnamentationType.AlternateTurn))
        .WithProcessors(turnAlternateTurnOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildThirtySecondNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.ThirtySecondNoteRun)
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DoubleTurn))
        )
        .WithProcessors(thirtySecondNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildThirtySecondSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.SixteenthNoteRun)
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Turn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.AlternateTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Pedal))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.SixteenthNoteRun))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Turn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.AlternateTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.DoubleTurn, OrnamentationType.Pedal))
        )
        .WithProcessors(thirtySecondSixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildMordentSixteenthNoteOrnamentationCleanerEngine() => PolicyEngineBuilder<OrnamentationCleaningItem>.Configure()
        .WithInputPolicies(
            new NotesContainTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.SixteenthNoteRun)
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Turn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.AlternateTurn))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.DecorateInterval))
                .Or(new NotesContainTargetOrnamentations(OrnamentationType.Mordent, OrnamentationType.Pedal))
        )
        .WithProcessors(mordentSixteenthNoteOrnamentationCleaner)
        .WithoutOutputPolicies()
        .Build();
}

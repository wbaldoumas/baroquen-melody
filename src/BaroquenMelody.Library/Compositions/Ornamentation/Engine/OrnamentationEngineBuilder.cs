using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine;

[ExcludeFromCodeCoverage(Justification = "Trivial builder methods.")]
internal sealed class OrnamentationEngineBuilder(CompositionConfiguration compositionConfiguration, IMusicalTimeSpanCalculator musicalTimeSpanCalculator)
{
    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildPassingToneEngine(),
            BuildDoublePassingToneEngine(),
            BuildDelayedDoublePassingToneEngine(),
            BuildDoubleTurnEngine(),
            BuildDelayedPassingToneEngine(),
            BuildSixteenthNoteRunEngine(),
            BuildTurnEngine(),
            BuildAlternateTurnEngine(),
            BuildDelayedThirtySecondNoteRunEngine()
        )
        .WithOutputPolicies(new CleanConflictingOrnamentations(new OrnamentationCleanerFactory()))
        .Build();

    public IPolicyEngine<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new IsRepeatedNote(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.PassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedPassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, TurnProcessor.Interval)
        )
        .WithProcessors(new TurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildAlternateTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, AlternateTurnProcessor.Interval)
        )
        .WithProcessors(new AlternateTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildSixteenthNoteRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, SixteenthNoteRunProcessor.Interval)
        )
        .WithProcessors(new SixteenthNoteRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedThirtySecondNoteRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(10),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DelayedThirtySecondNoteRunProcessor.Interval)
        )
        .WithProcessors(new DelayedThirtySecondNoteRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoubleTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(25),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DoubleTurnProcessor.Interval)
        )
        .WithProcessors(new DoubleTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoublePassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DoublePassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedDoublePassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedDoublePassingTone))
        .WithoutOutputPolicies()
        .Build();
}

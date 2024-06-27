using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine;

[ExcludeFromCodeCoverage(Justification = "Trivial builder methods.")]
internal sealed class OrnamentationEngineBuilder(CompositionConfiguration compositionConfiguration, IMusicalTimeSpanCalculator musicalTimeSpanCalculator)
{
    public IPolicyEngine<OrnamentationItem> Build() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildPassingToneEngine(),
            BuildDelayedPassingToneEngine(),
            BuildTurnEngine(),
            BuildSixteenthNoteRunEngine()
        )
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(
            new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.PassingTone)
        )
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(
            new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedPassingTone)
        )
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(
            new TurnProcessor(musicalTimeSpanCalculator, compositionConfiguration)
        )
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildSixteenthNoteRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(90),
            new HasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, SixteenthNoteRunProcessor.Interval)
        )
        .WithProcessors(
            new SixteenthNoteRunProcessor(musicalTimeSpanCalculator, compositionConfiguration)
        )
        .WithoutOutputPolicies()
        .Build();
}

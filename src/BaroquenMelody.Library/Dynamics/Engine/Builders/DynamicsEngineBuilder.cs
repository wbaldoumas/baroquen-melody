using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Dynamics.Engine.Policies.Input;
using BaroquenMelody.Library.Dynamics.Engine.Processors;
using BaroquenMelody.Library.Dynamics.Engine.Utilities;

namespace BaroquenMelody.Library.Dynamics.Engine.Builders;

internal sealed class DynamicsEngineBuilder(CompositionConfiguration configuration)
{
    private readonly IVelocityCalculator _velocityCalculator = new VelocityCalculator();

    private readonly IInputPolicy<DynamicsApplicationItem> _hasNotProcessedCurrentBeat = new Not<DynamicsApplicationItem>(new HasProcessedCurrentBeat());

    private readonly IInputPolicy<DynamicsApplicationItem> _instrumentIsPresentInCurrentBeat = new InstrumentIsPresentInCurrentBeat();

    private readonly IInputPolicy<DynamicsApplicationItem> _instrumentIsPresentInPreviousBeat = new InstrumentIsPresentInPreviousBeat();

    private readonly IInputPolicy<DynamicsApplicationItem> _instrumentIsNotPresentInPreviousBeat = new Not<DynamicsApplicationItem>(new InstrumentIsPresentInPreviousBeat());

    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();

    public IPolicyEngine<DynamicsApplicationItem> Build()
    {
        return PolicyEngineBuilder<DynamicsApplicationItem>.Configure()
            .WithoutInputPolicies()
            .WithProcessors(
                BuildInitialDynamicsProcessor(),
                BuildDefaultDynamicsProcessor()
            )
            .WithoutOutputPolicies()
            .Build();
    }

    private IProcessor<DynamicsApplicationItem> BuildInitialDynamicsProcessor() => PolicyEngineBuilder<DynamicsApplicationItem>.Configure()
            .WithInputPolicies(
                _hasNotProcessedCurrentBeat,
                _instrumentIsNotPresentInPreviousBeat,
                _instrumentIsPresentInCurrentBeat
            )
            .WithProcessors(new InitialDynamicsProcessor(configuration))
            .WithoutOutputPolicies()
            .Build();

    private IProcessor<DynamicsApplicationItem> BuildDefaultDynamicsProcessor() => PolicyEngineBuilder<DynamicsApplicationItem>
        .Configure()
        .WithInputPolicies(
            _hasNotProcessedCurrentBeat,
            _instrumentIsPresentInPreviousBeat,
            _instrumentIsPresentInCurrentBeat
        )
        .WithProcessors(
            new DefaultDynamicsProcessor(
                _velocityCalculator,
                _weightedRandomBooleanGenerator,
                configuration
            )
        )
        .WithoutOutputPolicies()
        .Build();
}

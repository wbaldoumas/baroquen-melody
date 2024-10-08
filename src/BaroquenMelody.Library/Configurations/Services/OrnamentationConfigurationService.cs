using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations.Services;

internal sealed class OrnamentationConfigurationService(
    IDispatcher dispatcher,
    IState<CompositionOrnamentationConfigurationState> state
) : IOrnamentationConfigurationService
{
    private const int MinProbability = 10;

    private const int MaxProbability = 100;

    private static readonly FrozenSet<OrnamentationType> _configurableOrnamentations = AggregateOrnamentationConfiguration
        .Default
        .Configurations
        .Select(configuration => configuration.OrnamentationType)
        .ToFrozenSet();

    public IEnumerable<OrnamentationType> ConfigurableOrnamentations => _configurableOrnamentations;

    public void ConfigureDefaults()
    {
        var ornamentationConfigurationsByOrnamentationType = AggregateOrnamentationConfiguration.Default.Configurations.ToDictionary(
            configuration => configuration.OrnamentationType,
            configuration => new OrnamentationConfiguration(configuration.OrnamentationType, configuration.Status, configuration.Probability)
        );

        dispatcher.Dispatch(new BatchUpdateCompositionOrnamentationConfiguration(ornamentationConfigurationsByOrnamentationType));
    }

    public void Randomize()
    {
        var ornamentationConfigurationsByOrnamentationType = new Dictionary<OrnamentationType, OrnamentationConfiguration>();

        foreach (var configuration in AggregateOrnamentationConfiguration.Default.Configurations)
        {
            if (state.Value.Configurations[configuration.OrnamentationType].IsFrozen)
            {
                ornamentationConfigurationsByOrnamentationType.Add(
                    configuration.OrnamentationType,
                    state.Value.Configurations[configuration.OrnamentationType]
                );

                continue;
            }

            var status = state.Value.Configurations[configuration.OrnamentationType].Status;
            var probability = ThreadLocalRandom.Next(MinProbability, MaxProbability + 1);

            ornamentationConfigurationsByOrnamentationType.Add(
                configuration.OrnamentationType,
                new OrnamentationConfiguration(configuration.OrnamentationType, status, probability)
            );
        }

        dispatcher.Dispatch(new BatchUpdateCompositionOrnamentationConfiguration(ornamentationConfigurationsByOrnamentationType));
    }
}

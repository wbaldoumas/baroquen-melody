using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class OrnamentationConfigurationService(IDispatcher dispatcher) : IOrnamentationConfigurationService
{
    private static readonly FrozenSet<OrnamentationType> _configurableOrnamentations = AggregateOrnamentationConfiguration
        .Default
        .Configurations
        .Select(configuration => configuration.OrnamentationType)
        .ToFrozenSet();

    public IEnumerable<OrnamentationType> ConfigurableOrnamentations => _configurableOrnamentations;

    public void ConfigureDefaults()
    {
        foreach (var configuration in AggregateOrnamentationConfiguration.Default.Configurations)
        {
            dispatcher.Dispatch(new UpdateCompositionOrnamentationConfiguration(configuration.OrnamentationType, configuration.IsEnabled, configuration.Probability));
        }
    }

    public void Randomize()
    {
        foreach (var configuration in AggregateOrnamentationConfiguration.Default.Configurations)
        {
            var isEnabled = ThreadLocalRandom.Next() % 2 == 0;
            var probability = ThreadLocalRandom.Next(0, 101);

            dispatcher.Dispatch(new UpdateCompositionOrnamentationConfiguration(configuration.OrnamentationType, isEnabled, probability));
        }
    }
}

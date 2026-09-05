using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record InstrumentConfigurationState(IDictionary<Instrument, InstrumentConfiguration> Configurations, IDictionary<Instrument, InstrumentConfiguration> LastUserAppliedConfigurations)
{
    private const int MinimumEnabledConfigurations = 1;

    public ISet<InstrumentConfiguration> EnabledConfigurations => Configurations.Values.Where(configuration => configuration.IsEnabled).ToHashSet();

    // An insertion-ordered HashSet, never a frozen set: a frozen set of records enumerates in hash-bucket order,
    // which varies with process history.
    public ISet<InstrumentConfiguration> AllConfigurations => Configurations.Values.ToHashSet();

    public InstrumentConfigurationState()
        : this(InstrumentConfiguration.DefaultConfigurations, InstrumentConfiguration.DefaultConfigurations)
    {
    }

    public InstrumentConfiguration? this[Instrument instrument] => Configurations.TryGetValue(instrument, out var configuration) ? configuration : null;

    /// <summary>
    ///     The last user-applied configurations with their ranges snapped to the closest notes of the given
    ///     scale — the configurations a key change re-applies. Both the key-change effect and any logic that
    ///     must anticipate a key change's outcome (e.g. rolling a ground bass pattern for a new key) consult
    ///     this single projection so they cannot drift apart.
    /// </summary>
    /// <param name="scale">The scale of the new key.</param>
    /// <returns>The snapped configurations.</returns>
    public IEnumerable<InstrumentConfiguration> LastUserAppliedConfigurationsSnappedTo(BaroquenScale scale) =>
        LastUserAppliedConfigurations.Values.Select(configuration => SnapToScale(configuration, scale));

    /// <summary>
    ///     The enabled configurations as they will stand after a key change to the given scale re-snaps the
    ///     last user-applied ranges.
    /// </summary>
    /// <param name="scale">The scale of the new key.</param>
    /// <returns>The enabled post-snap configurations.</returns>
    public ISet<InstrumentConfiguration> EnabledConfigurationsSnappedTo(BaroquenScale scale)
    {
        var configurations = new Dictionary<Instrument, InstrumentConfiguration>(Configurations);

        foreach (var snappedConfiguration in LastUserAppliedConfigurationsSnappedTo(scale))
        {
            configurations[snappedConfiguration.Instrument] = snappedConfiguration;
        }

        return configurations.Values.Where(configuration => configuration.IsEnabled).ToHashSet();
    }

    private static InstrumentConfiguration SnapToScale(InstrumentConfiguration configuration, BaroquenScale scale) => configuration with
    {
        MinNote = scale.GetNotes().OrderBy(note => Math.Abs(note.NoteNumber - configuration.MinNote.NoteNumber)).First(),
        MaxNote = scale.GetNotes().OrderBy(note => Math.Abs(note.NoteNumber - configuration.MaxNote.NoteNumber)).First()
    };

    public bool IsValid => EnabledConfigurations.Count >= MinimumEnabledConfigurations;

    public string ValidationMessage => IsValid ? string.Empty : "Cannot compose. Invalid instrumentation.";
}

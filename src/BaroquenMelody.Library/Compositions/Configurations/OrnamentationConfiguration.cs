using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums.Extensions;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///    Represents a configuration for an ornamentation in a composition.
/// </summary>
/// <param name="OrnamentationType">The type of ornamentation.</param>
/// <param name="Status">Whether the ornamentation is enabled, locked, or disabled.</param>
/// <param name="Probability">The probability that the ornamentation will be applied.</param>
public sealed record OrnamentationConfiguration(OrnamentationType OrnamentationType, ConfigurationStatus Status, int Probability)
{
    [JsonIgnore]
    public bool IsEnabled { get; } = Status.IsEnabled();

    [JsonIgnore]
    public bool IsFrozen { get; } = Status.IsFrozen();
}

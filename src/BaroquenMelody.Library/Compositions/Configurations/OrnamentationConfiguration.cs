using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///    Represents a configuration for an ornamentation in a composition.
/// </summary>
/// <param name="OrnamentationType">The type of ornamentation.</param>
/// <param name="IsEnabled">Whether the ornamentation is enabled.</param>
/// <param name="Probability">The probability that the ornamentation will be applied.</param>
public sealed record OrnamentationConfiguration(OrnamentationType OrnamentationType, bool IsEnabled, int Probability);

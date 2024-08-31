using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

/// <summary>
///     A service that deals with the configuration of ornamentations.
/// </summary>
public interface IOrnamentationConfigurationService
{
    /// <summary>
    ///     Which ornamentations can be configured by the user.
    /// </summary>
    IEnumerable<OrnamentationType> ConfigurableOrnamentations { get; }

    /// <summary>
    ///     Configure the default ornamentations.
    /// </summary>
    void ConfigureDefaults();

    /// <summary>
    ///     Randomly the ornamentations used in the composition.
    /// </summary>
    void Randomize();
}

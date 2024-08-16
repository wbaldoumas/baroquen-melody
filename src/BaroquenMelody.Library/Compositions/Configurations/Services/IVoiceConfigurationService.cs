using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

/// <summary>
///     A service that deals with the configuration of voices.
/// </summary>
public interface IVoiceConfigurationService
{
    /// <summary>
    ///     The voices that can be configured by the user.
    /// </summary>
    IEnumerable<Voice> ConfigurableVoices { get; }

    /// <summary>
    ///     Configure the default voice configurations.
    /// </summary>
    void ConfigureDefaults();
}

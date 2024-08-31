using BaroquenMelody.Library.Compositions.Rules.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

/// <summary>
///     A service that deals with the configuration of composition rules.
/// </summary>
public interface ICompositionRuleConfigurationService
{
    /// <summary>
    ///     The composition rules that can be configured by the user.
    /// </summary>
    IEnumerable<CompositionRule> ConfigurableCompositionRules { get; }

    /// <summary>
    ///     Configure the default composition rules.
    /// </summary>
    void ConfigureDefaults();

    /// <summary>
    ///     Randomize the configuration of the composition rules.
    /// </summary>
    void Randomize();
}

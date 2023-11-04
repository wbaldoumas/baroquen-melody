namespace BaroquenMelody.Library.Composition.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="VoiceConfigurations"> The voice configurations to be used in the composition. </param>
internal record CompositionConfiguration(
    ISet<VoiceConfiguration> VoiceConfigurations
);

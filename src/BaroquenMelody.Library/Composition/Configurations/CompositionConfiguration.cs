using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="VoiceConfigurations"> The voice configurations to be used in the composition. </param>
internal record CompositionConfiguration(ISet<VoiceConfiguration> VoiceConfigurations)
{
    public byte GetMinPitch(Voice voice) =>
        VoiceConfigurations.First(voiceConfiguration => voiceConfiguration.Voice == voice).MinPitch;

    public byte GetMaxPitch(Voice voice) =>
        VoiceConfigurations.First(voiceConfiguration => voiceConfiguration.Voice == voice).MaxPitch;
}

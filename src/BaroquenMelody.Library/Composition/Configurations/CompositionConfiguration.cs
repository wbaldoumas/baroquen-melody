using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="VoiceConfigurations"> The voice configurations to be used in the composition. </param>
internal record CompositionConfiguration(ISet<VoiceConfiguration> VoiceConfigurations)
{
    public bool IsPitchInVoiceRange(Voice voice, byte pitch) => pitch >= GetMinPitch(voice) && pitch <= GetMaxPitch(voice);

    private byte GetMinPitch(Voice voice) => VoiceConfigurations.First(x => x.Voice == voice).MinPitch;

    private byte GetMaxPitch(Voice voice) => VoiceConfigurations.First(x => x.Voice == voice).MaxPitch;
}

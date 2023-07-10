using BaroquenMelody.Library.Composition.Enums;

namespace BaroquenMelody.Library.Composition.Configurations;

/// <summary>
///    The voice configuration. Allowing for the configuration of the pitch range for a given voice.
/// </summary>
/// <param name="Voice"> The voice to be configured. </param>
/// <param name="MinPitch"> The voice's minimum pitch value. </param>
/// <param name="MaxPitch"> The voice's maximum pitch value. </param>
internal record VoiceConfiguration(
    Voice Voice,
    byte MinPitch,
    byte MaxPitch
);

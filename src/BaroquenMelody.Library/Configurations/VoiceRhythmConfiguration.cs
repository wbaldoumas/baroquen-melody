using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures per-voice rhythm roles over phrase-length blocks of the fugal composition body: a held voice
///     moves once per measure with its repeats tied into sustained tones, a florid voice attracts more of the
///     subdividing ornament figures, and the remaining voices keep the standard texture. The fugal exposition
///     and ending and the ground bass form take no roles.
/// </summary>
/// <param name="Enabled"> Whether per-voice rhythm roles are assigned at all. When <see langword="false"/>, every voice composes with the standard texture. </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record VoiceRhythmConfiguration(bool Enabled)
{
    public static VoiceRhythmConfiguration Default { get; } = new(Enabled: true);
}

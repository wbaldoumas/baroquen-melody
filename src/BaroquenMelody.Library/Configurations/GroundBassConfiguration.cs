using BaroquenMelody.Library.Forms.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures the ground bass form: a short bass pattern announced alone, then repeated under a freshly
///     composed upper texture, closing with a cadence onto the tonic - the passacaglia's shape. When enabled
///     it replaces the fugal form for the whole composition; the fugue remains the default.
/// </summary>
/// <param name="Enabled"> Whether the composition takes the ground bass form instead of the fugal form. </param>
/// <param name="Pattern">
///     The specific bank pattern to state, or <see langword="null"/> to let the composer draw among the
///     patterns that fit the lowest voice's range. A configured pattern the range cannot host yields no
///     plan, falling back to the fugal form the way an empty bank does.
/// </param>
/// <param name="Modulate">
///     Whether the composition may journey to the relative key for a block of middle statements before
///     returning home. The journey happens in the Ionian and Aeolian modes when at least four statements
///     are planned and the relative-key rendering both fits the bass range and meets the home ground at
///     singable seams; when any condition fails the composition simply stays in the home key.
/// </param>
/// <param name="Divisions">
///     Whether the accompanied statements escalate in the divisions idiom: figuration intensifying
///     statement over statement toward the close, one upper voice per statement carrying the
///     concentration, the ground line tying into a sustained tread, and statement-level terraced dynamics.
///     Requires the voice-rhythm role machinery (<see cref="VoiceRhythmConfiguration"/>) to be enabled;
///     the opening announcement and the final cadence never escalate, and a ground with no upper voices
///     only takes the sustained tread.
/// </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record GroundBassConfiguration(bool Enabled, GroundBass? Pattern = null, bool Modulate = true, bool Divisions = true)
{
    public static GroundBassConfiguration Default { get; } = new(Enabled: false);
}

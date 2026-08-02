using BaroquenMelody.Library.Configurations.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     The configuration of the fugal body's accompaniment texture. Textures ride the voice-rhythm role
///     machinery, so <see cref="VoiceRhythmConfiguration.Enabled"/> is their master switch; they apply to the
///     fugue form only (the ground bass form's texture is its divisions), need at least two voices, and shape
///     figuration through the user's per-ornamentation switches - a globally disabled figure is never
///     resurrected by a texture, so heavily pruned ornamentations degrade a texture toward
///     <see cref="TextureType.Chordal"/>.
/// </summary>
/// <param name="Texture">The accompaniment texture to render; <see cref="TextureType.None"/> keeps today's fabric.</param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record TextureConfiguration(TextureType Texture)
{
    public static TextureConfiguration Default { get; } = new(TextureType.None);
}

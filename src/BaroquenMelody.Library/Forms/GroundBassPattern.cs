using BaroquenMelody.Library.Forms.Enums;

namespace BaroquenMelody.Library.Forms;

/// <summary>
///     A ground bass pattern: a short repeating bass line expressed mode-agnostically as scale-step offsets
///     from an anchor tonic, so the same ground renders into any key, mode, and bass register.
/// </summary>
/// <param name="Identifier"> The pattern's identity in the bank. </param>
/// <param name="ScaleStepOffsets"> One scale-step offset from the anchor tonic per ground note. </param>
/// <remarks>
///     Every pattern in the bank starts on the tonic (offset 0), ends on the dominant degree (offset -3, the
///     dominant a fourth below the tonic) so each statement seam forms a dominant-to-tonic strong-slot pair,
///     and has an even note count so statements fill whole measures at two slots per ground note.
/// </remarks>
internal sealed record GroundBassPattern(GroundBass Identifier, IReadOnlyList<int> ScaleStepOffsets)
{
    /// <summary>
    ///     The built-in patterns. The descending tetrachord's three-step span fits every legal bass range that
    ///     offers a tonic anchor; the wider grounds participate only where the bass range can host them.
    /// </summary>
    public static readonly IReadOnlyList<GroundBassPattern> Bank =
    [
        new(GroundBass.DescendingTetrachord, [0, -1, -2, -3]),
        new(GroundBass.Romanesca, [0, -3, -2, -5, -4, -7, -4, -3]),
        new(GroundBass.CadentialGround, [0, -3, -5, -4, 0, -2, -4, -3])
    ];
}

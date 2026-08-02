using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Rhythm;

/// <summary>
///     The fugal body's single per-voice rhythm identity authority, answering in one of two modes. With no
///     accompaniment texture configured, roles rotate over phrase-length blocks: which voice is held (moving
///     once per measure), which is florid (attracting more subdividing figures), and where the held voice's
///     note must be pinned during the body walk. With a texture configured, the rotation yields to one static
///     whole-composition assignment - melody on top, figuration at the bottom, pads between - and the
///     rotating answers decline so the two modes can never overlap.
/// </summary>
internal interface IVoiceRhythmScheduler
{
    /// <summary>
    ///     Determine which instrument, if any, carries the held role for the given measure.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="heldInstrument">The instrument holding through the measure, when one exists.</param>
    /// <returns>Whether a held instrument exists for the measure.</returns>
    bool TryGetHeldInstrument(int measureIndex, out Instrument heldInstrument);

    /// <summary>
    ///     Determine which instrument, if any, must have its previous note pinned at the given beat.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="beatIndex">The zero-based index of the beat within its measure.</param>
    /// <param name="pinnedInstrument">The instrument whose previous note is pinned at the beat, when one exists.</param>
    /// <returns>Whether an instrument is pinned at the beat.</returns>
    bool TryGetPinnedInstrument(int measureIndex, int beatIndex, out Instrument pinnedInstrument);

    /// <summary>
    ///     Determine which instrument, if any, carries the florid role for the given measure.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="floridInstrument">The instrument attracting subdividing figures in the measure, when one exists.</param>
    /// <returns>Whether a florid instrument exists for the measure.</returns>
    bool TryGetFloridInstrument(int measureIndex, out Instrument floridInstrument);

    /// <summary>
    ///     Determine the given instrument's part in the active accompaniment texture, when one is active.
    /// </summary>
    /// <param name="instrument">The instrument to resolve.</param>
    /// <param name="textureRole">The instrument's texture role, when a texture is active.</param>
    /// <returns>Whether a texture is active and the instrument participates in it.</returns>
    bool TryGetTextureRole(Instrument instrument, out TextureRole textureRole);

    /// <summary>
    ///     Resolve the decoration sequence for an active texture: melody first, figuration last, so the
    ///     ornamentation cleaners' just-decorated-loses rule resolves a dissonant coincidence between two
    ///     figures placed in the same decoration pass in the melody's favor. (A melody figure placed by a
    ///     later pass can still be cleaned against an accompaniment figure that survives from an earlier
    ///     one - the ordering guarantee is per pass, not global.)
    /// </summary>
    /// <param name="decorationOrder">The register-ordered instruments, when a texture is active.</param>
    /// <returns>Whether a texture is active.</returns>
    bool TryGetTextureDecorationOrder(out IReadOnlyList<Instrument> decorationOrder);
}

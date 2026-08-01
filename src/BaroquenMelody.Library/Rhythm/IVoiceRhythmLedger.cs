using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rhythm;

/// <summary>
///     Records which of the body walk's emitted notes carry the held or florid rhythm role, so the decoration
///     and sustain passes can treat exactly those notes differently. Notes never recorded — the exposition,
///     the ending, the ground bass form, and the phraser's deep copies — take the standard behavior.
/// </summary>
internal interface IVoiceRhythmLedger
{
    /// <summary>
    ///     Forget every recorded note, ready for a fresh composition.
    /// </summary>
    void Clear();

    /// <summary>
    ///     Record a note emitted for the held voice of a held measure.
    /// </summary>
    /// <param name="note">The exact note instance the body walk emitted.</param>
    void RecordHeldNote(BaroquenNote note);

    /// <summary>
    ///     Record a note emitted for the florid voice of its block.
    /// </summary>
    /// <param name="note">The exact note instance the body walk emitted.</param>
    void RecordFloridNote(BaroquenNote note);

    /// <summary>
    ///     Determine whether the given note instance was recorded as held.
    /// </summary>
    /// <param name="note">The note instance to look up.</param>
    /// <returns>Whether the note was recorded as held.</returns>
    bool IsHeldNote(BaroquenNote note);

    /// <summary>
    ///     Determine whether the given note instance was recorded as florid.
    /// </summary>
    /// <param name="note">The note instance to look up.</param>
    /// <returns>Whether the note was recorded as florid.</returns>
    bool IsFloridNote(BaroquenNote note);
}

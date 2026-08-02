using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rhythm;

/// <summary>
///     Records which of the composed notes carry a rhythm role, so the decoration and sustain passes can
///     treat exactly those notes differently: the fugal body walk records its held and florid voices, and
///     the ground bass form records its division roles — the ground line held, and each accompanied
///     statement's upper-voice notes carrying an escalation intensity. Notes never recorded — the fugal
///     exposition and ending, the ground's opening announcement and close, and every deep copy — take the
///     standard behavior.
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

    /// <summary>
    ///     Record the division-escalation intensity for a note of an accompanied ground statement. Recording
    ///     the same instance again overwrites the earlier intensity.
    /// </summary>
    /// <param name="note">The exact note instance the ground walk emitted.</param>
    /// <param name="intensity">The statement's escalation intensity, as a percentage weight scale.</param>
    void RecordDivisionIntensity(BaroquenNote note, int intensity);

    /// <summary>
    ///     Look up the division-escalation intensity recorded for the given note instance.
    /// </summary>
    /// <param name="note">The note instance to look up.</param>
    /// <param name="intensity">The recorded intensity, when one exists.</param>
    /// <returns>Whether an intensity was recorded for the note.</returns>
    bool TryGetDivisionIntensity(BaroquenNote note, out int intensity);
}

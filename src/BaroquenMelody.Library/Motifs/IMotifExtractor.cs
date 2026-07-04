using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     Derives a voice-agnostic <see cref="Motif"/> from a single voice's ordered principal-note sequence.
/// </summary>
internal interface IMotifExtractor
{
    /// <summary>
    ///     Extracts the principal-skeleton motif from the given voice line, ignoring all ornamentation.
    /// </summary>
    /// <remarks>
    ///     Each note's own <c>MusicalTimeSpan</c> is taken verbatim as its gesture duration; the child
    ///     <c>Ornamentations</c> and <c>OrnamentationType</c> are ignored. Pass the pre-ornamentation principal line:
    ///     a note's <c>MusicalTimeSpan</c> may already be shortened by decoration, so extracting from a decorated line
    ///     yields decorated durations rather than the structural skeleton.
    /// </remarks>
    /// <param name="voiceLine">The ordered principal notes of one voice. Every note must be in the configured scale.</param>
    /// <returns>The extracted, voice-agnostic <see cref="Motif"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="voiceLine"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any note in <paramref name="voiceLine"/> is not in the configured scale.</exception>
    Motif Extract(IReadOnlyList<BaroquenNote> voiceLine);
}

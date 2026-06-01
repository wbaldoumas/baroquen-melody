using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.MusicTheory;

/// <summary>
///     Produces the fugal answer for a subject: the subject transposed to the dominant, with the opening tonic/dominant
///     head adjusted so the answer remains in key (a real or tonal answer).
/// </summary>
internal interface IFugalAnswerStrategy
{
    /// <summary>
    ///     Generates the fugal answer for the given subject.
    /// </summary>
    /// <param name="subject">The subject to answer.</param>
    /// <returns>The answer notes, in the subject's instrument and rhythm.</returns>
    IReadOnlyList<BaroquenNote> GenerateAnswer(IReadOnlyList<BaroquenNote> subject);
}

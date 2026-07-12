using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Ornamentation;

/// <summary>
///     Applies the idiomatic cadential trill to the dominant chord of an authentic cadence.
/// </summary>
internal interface ICadentialTrillApplicator
{
    /// <summary>
    ///     Trill the leading-tone voice of the penultimate chord when the two chords form an authentic cadence.
    /// </summary>
    /// <param name="penultimateChord">The chord approaching the cadence, whose leading-tone voice is trilled.</param>
    /// <param name="finalChord">The chord of arrival.</param>
    void ApplyTrill(BaroquenChord penultimateChord, BaroquenChord finalChord);
}

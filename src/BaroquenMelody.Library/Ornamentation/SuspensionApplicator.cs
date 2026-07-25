using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Ornamentation;

/// <inheritdoc cref="ISuspensionApplicator"/>
/// <remarks>
///     A suspension is the dissonance treatment the ornamentation engine cannot produce: the appoggiatura
///     re-articulates its accented dissonance on the beat, while a suspension ties the preparation - the
///     voice's own previous chord tone, consonant by construction - across the harmonic change and delays the
///     resolution it was already going to sound. No new pitches are introduced; the figure is a time-shift at
///     the barline, so instrument ranges and the diatonic invariant are untouched. Eligibility is purely
///     harmonic, and an applied suspension replaces whatever surface ornamentation the two notes carried:
///     structural dissonance treatment outranks optional decoration, the same precedence the cadential trill
///     takes at cadences. Only a deliberately placed trill and notes already inside a suspension figure are
///     left alone. Runs after the ending is composed
///     and before the sustain pass, where a same-pitch predecessor may absorb the preparation into a longer
///     tie, which only extends the figure backwards. The composition's final measure is left untouched so the
///     crafted cadence rings pure.
/// </remarks>
internal sealed class SuspensionApplicator(
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    CompositionConfiguration compositionConfiguration
) : ISuspensionApplicator
{
    private readonly SuspensionConfiguration _suspensionConfiguration =
        compositionConfiguration.SuspensionConfiguration ?? SuspensionConfiguration.Default;

    public void ApplySuspensions(Composition composition)
    {
        if (!_suspensionConfiguration.Enabled)
        {
            return;
        }

        for (var measureIndex = 0; measureIndex < composition.Measures.Count - 1; measureIndex++)
        {
            var measure = composition.Measures[measureIndex];

            // Resolutions land only on the strong slots: mid-measure (slot 2, prepared by slot 1)...
            if (measure.Beats.Count > 2)
            {
                TryApplySuspensions(measure.Beats[1], measure.Beats[2]);
            }

            // ...and the next downbeat (slot 0, prepared by the final beat of this measure), unless the next
            // measure is the composition's final one.
            if (measureIndex < composition.Measures.Count - 2)
            {
                var nextMeasure = composition.Measures[measureIndex + 1];

                if (measure.Beats.Count > 0 && nextMeasure.Beats.Count > 0)
                {
                    TryApplySuspensions(measure.Beats[^1], nextMeasure.Beats[0]);
                }
            }
        }
    }

    private void TryApplySuspensions(Beat preparationBeat, Beat resolutionBeat)
    {
        foreach (var instrument in compositionConfiguration.Instruments)
        {
            if (!preparationBeat.Chord.ContainsInstrument(instrument) || !resolutionBeat.Chord.ContainsInstrument(instrument))
            {
                continue;
            }

            var preparation = preparationBeat.Chord[instrument];
            var resolution = resolutionBeat.Chord[instrument];

            // A deliberately placed trill (the cadential figure) is the one ornament a suspension must not
            // displace, and a note already participating in a suspension figure must never be restamped: in
            // the short leftover measures the ending composer can leave mid-composition, the cross-barline
            // pair's preparation aliases a beat the mid-measure pair may have just resolved.
            if (IsProtected(preparation) || IsProtected(resolution))
            {
                continue;
            }

            // The preparation must sit exactly one diatonic step above the chord tone it resolves onto, and
            // must genuinely clash with the new chord - a preparation whose pitch class survives into the new
            // harmony is a common tone, not a suspension.
            var preparationScaleIndex = compositionConfiguration.Scale.IndexOf(preparation);

            if (preparationScaleIndex <= 0 || compositionConfiguration.Scale.IndexOf(resolution) != preparationScaleIndex - 1)
            {
                continue;
            }

            if (resolutionBeat.Chord.Notes.Exists(note => note.NoteName == preparation.NoteName))
            {
                continue;
            }

            if (!weightedRandomBooleanGenerator.IsTrue(_suspensionConfiguration.Probability))
            {
                continue;
            }

            var halfBeat = compositionConfiguration.DefaultNoteTimeSpan / 2;

            preparation.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
            preparation.MusicalTimeSpan = compositionConfiguration.DefaultNoteTimeSpan + halfBeat;
            preparation.OrnamentationType = OrnamentationType.Suspension;

            resolution.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
            resolution.MusicalTimeSpan = halfBeat;
            resolution.OrnamentationType = OrnamentationType.SuspensionResolution;
        }
    }

    private static bool IsProtected(BaroquenNote note) => note.OrnamentationType
        is OrnamentationType.Trill
        or OrnamentationType.Suspension
        or OrnamentationType.SuspensionResolution;
}

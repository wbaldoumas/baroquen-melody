using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Motifs.Enums;

namespace BaroquenMelody.Library.Motifs;

/// <inheritdoc cref="IMotifDeveloper"/>
internal sealed class MotifDeveloper(
    IMotifApplicator motifApplicator,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IRandomProvider randomProvider,
    CompositionConfiguration compositionConfiguration
) : IMotifDeveloper
{
    public List<Measure>? TryDevelop(MotifBank motifBank, IReadOnlyList<Measure> phrase)
    {
        ArgumentNullException.ThrowIfNull(motifBank);
        ArgumentNullException.ThrowIfNull(phrase);

        var configuration = compositionConfiguration.MotifDevelopmentConfiguration ?? MotifDevelopmentConfiguration.Default;

        // Draw nothing unless development is genuinely enabled, so a disabled config leaves the legacy random stream intact.
        if (!configuration.Enabled || configuration.AllowedTransforms.Count == 0 || phrase.Count == 0)
        {
            return null;
        }

        if (!weightedRandomBooleanGenerator.IsTrue(configuration.DevelopmentProbability))
        {
            return null;
        }

        var firstMeasure = phrase[0];

        // Candidate voices are drawn from the deterministically ordered instrument list (never the bank's frozen key order)
        // and limited to voices that both have a cataloged motif and sound on the phrase's first measure.
        var candidateVoices = compositionConfiguration.Instruments
            .Where(instrument => motifBank.Contains(instrument) && firstMeasure.Beats.Any(beat => beat.ContainsInstrument(instrument)))
            .ToList();

        if (candidateVoices.Count == 0)
        {
            return null;
        }

        var voice = candidateVoices[randomProvider.Next(candidateVoices.Count)];
        var transform = configuration.AllowedTransforms[randomProvider.Next(configuration.AllowedTransforms.Count)];

        var targetBeatIndexes = Enumerable.Range(0, firstMeasure.Beats.Count)
            .Where(beatIndex => firstMeasure.Beats[beatIndex].ContainsInstrument(voice))
            .ToList();

        // Re-anchor to the phrase's own first note for this voice (not the bank's exposition anchor), so the developed head
        // equals the verbatim head and the seam chord stays identical.
        var anchorScaleIndex = compositionConfiguration.Scale.IndexOf(firstMeasure.Beats[targetBeatIndexes[0]][voice]);
        var developedMotif = ApplyTransform(transform, motifBank.Get(voice).Motif);
        var developedNotes = motifApplicator.Apply(new AnchoredMotif(developedMotif, anchorScaleIndex, voice));

        // The developed line must tile the voice's beats one-to-one; otherwise (a staggered or rest-bearing entry, or a
        // length-changing transform) fall back to a verbatim restatement.
        if (developedNotes.Count != targetBeatIndexes.Count)
        {
            return null;
        }

        var developedPhrase = phrase.Select(static measure => new Measure(measure)).ToList();
        var developedFirstMeasure = developedPhrase[0];

        for (var noteIndex = 0; noteIndex < targetBeatIndexes.Count; ++noteIndex)
        {
            var beatIndex = targetBeatIndexes[noteIndex];
            var existingChord = developedFirstMeasure.Beats[beatIndex].Chord;

            // A clean principal note re-stamped to the default duration so it tiles the grid; the trailing decoration pass
            // re-ornaments it exactly like body material.
            var developedNote = new BaroquenNote(voice, developedNotes[noteIndex].Raw, compositionConfiguration.DefaultNoteTimeSpan);

            // Rebuild the chord rather than mutating its notes in place, so its cached instrument lookup stays consistent.
            var notes = existingChord.Notes
                .Select(note => note.Instrument == voice ? developedNote : new BaroquenNote(note))
                .ToList();

            developedFirstMeasure.Beats[beatIndex] = new Beat(new BaroquenChord(notes));
        }

        return developedPhrase;
    }

    // Only the pitch-preserving transforms are wired for now; the duration- and length-changing transforms are reserved.
    private static Motif ApplyTransform(MotifTransform transform, Motif motif) => transform switch
    {
        MotifTransform.Invert => MotifTransformations.Invert(motif),
        MotifTransform.Retrograde => MotifTransformations.Retrograde(motif),
        _ => MotifTransformations.Identity(motif)
    };
}

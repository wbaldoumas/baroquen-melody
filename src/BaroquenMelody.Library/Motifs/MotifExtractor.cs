using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Motifs;

/// <inheritdoc cref="IMotifExtractor"/>
internal sealed class MotifExtractor(CompositionConfiguration compositionConfiguration) : IMotifExtractor
{
    /// <inheritdoc/>
    public Motif Extract(IReadOnlyList<BaroquenNote> voiceLine)
    {
        ArgumentNullException.ThrowIfNull(voiceLine);

        var scale = compositionConfiguration.Scale;
        var gestures = new List<MotivicGesture>(voiceLine.Count);
        var previousScaleIndex = -1;

        for (var index = 0; index < voiceLine.Count; ++index)
        {
            var note = voiceLine[index];
            var scaleIndex = scale.IndexOf(note);

            if (scaleIndex < 0)
            {
                throw new ArgumentException(
                    $"The principal note '{note.Raw}' is not in the configured scale; a scale-step motif cannot faithfully represent a chromatic pitch.",
                    nameof(voiceLine)
                );
            }

            // The head note has no predecessor, so it anchors the motif with a zero delta; every later gesture is the
            // signed scale-step distance from the previous principal note. Ornamentations are intentionally ignored.
            gestures.Add(new MotivicGesture(index == 0 ? 0 : scaleIndex - previousScaleIndex, note.MusicalTimeSpan));

            previousScaleIndex = scaleIndex;
        }

        // Hand the gestures over as an array so the IReadOnlyList cannot be downcast to List and mutated, protecting
        // the documented Gestures[0].ScaleStepDelta == 0 invariant.
        return new Motif(gestures.ToArray());
    }
}

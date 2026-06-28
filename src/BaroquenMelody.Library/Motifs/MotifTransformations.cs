using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     Pure, deterministic <see cref="Motif"/>-to-<see cref="Motif"/> developmental transforms over the purely
///     relative (scale-step delta + duration) gesture representation. Every method returns a new <see cref="Motif"/>,
///     never mutates its input, and preserves the head-zero invariant (<c>Gestures[0].ScaleStepDelta == 0</c>).
///     Transforms carry no anchor, instrument, or absolute pitch; a pitch shift (transposition / sequence) is an
///     apply-time concern realized through the anchor, so it is deliberately absent here.
/// </summary>
internal static class MotifTransformations
{
    /// <summary>
    ///     The finest rhythmic value the onset pipeline can faithfully place (mirrors
    ///     <c>NoteOnsetCalculator.Resolution</c> == 1/32, duplicated locally to avoid coupling the Motifs namespace to
    ///     the ornamentation pipeline). Diminution may not produce a shorter duration; the boundary value is allowed.
    /// </summary>
    private static readonly MusicalTimeSpan MinimumDuration = MusicalTimeSpan.ThirtySecond;

    /// <summary>The verbatim oracle: returns the motif unchanged (safe because <see cref="Motif"/> is immutable).</summary>
    /// <param name="motif">The motif to return.</param>
    /// <returns>The same <paramref name="motif"/> instance.</returns>
    public static Motif Identity(Motif motif)
    {
        ArgumentNullException.ThrowIfNull(motif);

        return motif;
    }

    /// <summary>
    ///     Real (anchor-relative) inversion: negates every delta to mirror the contour about the head. The head delta
    ///     stays 0 and durations are unchanged.
    /// </summary>
    /// <param name="motif">The motif to invert.</param>
    /// <returns>The inverted motif.</returns>
    public static Motif Invert(Motif motif)
    {
        ArgumentNullException.ThrowIfNull(motif);

        var gestures = motif.Gestures;
        var inverted = new MotivicGesture[gestures.Count];

        for (var index = 0; index < gestures.Count; ++index)
        {
            // The head delta is 0 and -0 == 0, so the head-zero invariant is preserved automatically.
            inverted[index] = gestures[index] with { ScaleStepDelta = -gestures[index].ScaleStepDelta };
        }

        return new Motif(inverted);
    }

    /// <summary>
    ///     Retrograde: rebuilds the motif back-to-front. It is NOT a list reverse, because each gesture pairs an inbound
    ///     delta with that note's own duration. Durations are reversed and deltas re-derived (head 0, then the negated
    ///     reverse of the body), preserving the head-zero invariant.
    /// </summary>
    /// <param name="motif">The motif to retrograde.</param>
    /// <returns>The retrograde motif.</returns>
    public static Motif Retrograde(Motif motif)
    {
        ArgumentNullException.ThrowIfNull(motif);

        var gestures = motif.Gestures;
        var count = gestures.Count;
        var retrograde = new MotivicGesture[count];

        for (var index = 0; index < count; ++index)
        {
            // Reading pitches back-to-front, the new head re-anchors at delta 0 and every later delta is the negated
            // delta of the mirrored note; the duration is that mirrored note's own duration.
            var delta = index == 0 ? 0 : -gestures[count - index].ScaleStepDelta;

            retrograde[index] = new MotivicGesture(delta, gestures[count - 1 - index].Duration);
        }

        return new Motif(retrograde);
    }

    /// <summary>
    ///     Augmentation: lengthens every duration by the integer <paramref name="factor"/> via exact rational scaling;
    ///     deltas are unchanged.
    /// </summary>
    /// <param name="motif">The motif to augment.</param>
    /// <param name="factor">The positive integer factor to multiply each duration by.</param>
    /// <returns>The augmented motif.</returns>
    public static Motif Augment(Motif motif, int factor)
    {
        ArgumentNullException.ThrowIfNull(motif);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(factor);

        var gestures = motif.Gestures;
        var augmented = new MotivicGesture[gestures.Count];

        for (var index = 0; index < gestures.Count; ++index)
        {
            // The long-typed operator * scales the numerator exactly; the lossy double Multiply overload is avoided.
            augmented[index] = gestures[index] with { Duration = gestures[index].Duration * factor };
        }

        return new Motif(augmented);
    }

    /// <summary>
    ///     Diminution: shortens every duration by the integer <paramref name="factor"/> via exact rational scaling;
    ///     deltas are unchanged. Throws if any resulting duration would fall below <see cref="MinimumDuration"/>.
    /// </summary>
    /// <param name="motif">The motif to diminish.</param>
    /// <param name="factor">The positive integer factor to divide each duration by.</param>
    /// <returns>The diminished motif.</returns>
    public static Motif Diminish(Motif motif, int factor)
    {
        ArgumentNullException.ThrowIfNull(motif);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(factor);

        var gestures = motif.Gestures;
        var diminished = new MotivicGesture[gestures.Count];

        for (var index = 0; index < gestures.Count; ++index)
        {
            // The long-typed operator / scales the denominator exactly; the lossy double Divide overload is avoided.
            var duration = gestures[index].Duration / factor;

            if (duration < MinimumDuration)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(factor),
                    factor,
                    $"Diminishing by {factor} would shorten a {gestures[index].Duration} gesture below the minimum representable duration of {MinimumDuration}."
                );
            }

            diminished[index] = gestures[index] with { Duration = duration };
        }

        return new Motif(diminished);
    }

    /// <summary>
    ///     Fragmentation: extracts the contiguous sub-motif <c>[start, start + length)</c>, re-zeroing the new head's
    ///     inbound delta so the head-zero invariant holds; all other deltas and all durations are verbatim. This is a
    ///     lossy, non-invertible projection.
    /// </summary>
    /// <param name="motif">The motif to slice.</param>
    /// <param name="start">The zero-based index of the first gesture to keep.</param>
    /// <param name="length">The number of gestures to keep (at least one).</param>
    /// <returns>The fragment motif.</returns>
    public static Motif Fragment(Motif motif, int start, int length)
    {
        ArgumentNullException.ThrowIfNull(motif);
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        var gestures = motif.Gestures;

        ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, gestures.Count);

        var fragment = new MotivicGesture[length];

        for (var index = 0; index < length; ++index)
        {
            var source = gestures[start + index];

            // The slice's head drops its inbound interval and re-anchors at delta 0; later gestures keep their deltas.
            fragment[index] = index == 0 ? source with { ScaleStepDelta = 0 } : source;
        }

        return new Motif(fragment);
    }
}

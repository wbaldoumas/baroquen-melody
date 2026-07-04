namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     A voice-agnostic melodic motif: an ordered, purely relative sequence of <see cref="MotivicGesture"/>s
///     (scale-step deltas + durations). It stores no absolute pitch, no instrument, and no anchor, so the same motif
///     can be re-anchored to any starting pitch, instrument, and measure at application time.
/// </summary>
/// <remarks>
///     Equality is reference-based on <see cref="Gestures"/> (the synthesized record equality does not compare the
///     list structurally), so a <see cref="Motif"/> must NOT be used as a dictionary or set key until structural
///     equality is introduced; compare <see cref="Gestures"/> element-wise instead. Because each gesture pairs an
///     inter-note delta with that note's own duration, transforms such as retrograde must rebuild the gesture list
///     (reverse and negate deltas) rather than simply reverse it.
/// </remarks>
/// <param name="Gestures">
///     The ordered gestures, one per principal note. By invariant <c>Gestures[0].ScaleStepDelta == 0</c> (the head has
///     no predecessor). An empty list represents the empty motif.
/// </param>
internal sealed record Motif(IReadOnlyList<MotivicGesture> Gestures);

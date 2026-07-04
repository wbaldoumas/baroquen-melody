using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     A single step of a <see cref="Motif"/>: the diatonic scale-step interval from the previous principal note,
///     paired with this principal note's duration.
/// </summary>
/// <param name="ScaleStepDelta">
///     Signed diatonic distance, in scale steps, from the previous principal note to this one
///     (<c>scale.IndexOf(current) - scale.IndexOf(previous)</c>). The motif head has no predecessor, so its delta is 0.
///     Deltas are unique and monotonic across octaves (an ascending octave in a seven-note scale is +7).
/// </param>
/// <param name="Duration">The principal note's rhythmic value.</param>
internal sealed record MotivicGesture(int ScaleStepDelta, MusicalTimeSpan Duration);

using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using System.Runtime.InteropServices;

namespace BaroquenMelody.Library.MusicTheory;

/// <summary>
///     The chord tones (root, third, and fifth) of a diatonic triad, expressed as note names within a given scale.
/// </summary>
/// <param name="Root">The root of the triad.</param>
/// <param name="Third">The third of the triad.</param>
/// <param name="Fifth">The fifth of the triad.</param>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct ChordTriad(NoteName Root, NoteName Third, NoteName Fifth)
{
    /// <summary>
    ///     Resolve the chord tones of the triad built on the given chord number within the given scale.
    /// </summary>
    /// <param name="scale">The scale to resolve the triad's note names from.</param>
    /// <param name="chordNumber">The chord number identifying the triad.</param>
    /// <returns>The resolved <see cref="ChordTriad"/>, or <see langword="null"/> for <see cref="ChordNumber.Unknown"/>.</returns>
    public static ChordTriad? FromChordNumber(BaroquenScale scale, ChordNumber chordNumber) => chordNumber switch
    {
        ChordNumber.I => new ChordTriad(scale.Tonic, scale.Mediant, scale.Dominant),
        ChordNumber.II => new ChordTriad(scale.Supertonic, scale.Subdominant, scale.Submediant),
        ChordNumber.III => new ChordTriad(scale.Mediant, scale.Dominant, scale.LeadingTone),
        ChordNumber.IV => new ChordTriad(scale.Subdominant, scale.Submediant, scale.Tonic),
        ChordNumber.V => new ChordTriad(scale.Dominant, scale.LeadingTone, scale.Supertonic),
        ChordNumber.VI => new ChordTriad(scale.Submediant, scale.Tonic, scale.Mediant),
        ChordNumber.VII => new ChordTriad(scale.LeadingTone, scale.Supertonic, scale.Subdominant),
        _ => null
    };
}

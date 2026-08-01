using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Forms;

/// <summary>
///     A contiguous run of ground statements sharing one key: the tonal plan's concrete section. A home-only
///     plan is a single section spanning every statement; a modulating plan is a home departure, a
///     relative-key middle, and a home return.
/// </summary>
/// <param name="Tonic"> The section's tonic. </param>
/// <param name="Mode"> The section's mode. </param>
/// <param name="FirstStatement"> The first statement index the section spans, inclusive. </param>
/// <param name="LastStatement"> The last statement index the section spans, inclusive. </param>
/// <param name="BassNotes"> The ground pattern rendered into the bass register against this section's scale. </param>
/// <remarks>
///     Relative keys share one pitch set, which is what makes the section seam safe for every component that
///     resolves context notes through a scale's note list: any note either section emits exists in both
///     scales. A section in a non-relative key must not be constructed until the candidate-generation and
///     ornamentation surfaces learn to resolve out-of-scale context notes.
/// </remarks>
internal sealed record TonalSection(
    NoteName Tonic,
    Mode Mode,
    int FirstStatement,
    int LastStatement,
    IReadOnlyList<Note> BassNotes);

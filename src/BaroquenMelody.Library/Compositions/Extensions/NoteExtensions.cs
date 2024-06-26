using BaroquenMelody.Library.Compositions.Enums.Extensions;
using Melanchall.DryWetMidi.MusicTheory;
using Interval = BaroquenMelody.Library.Compositions.Enums.Interval;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extensions for the <see cref="Note"/> class.
/// </summary>
internal static class NoteExtensions
{
    public static bool IsDissonantWith(this Note note, Note otherNote) => IntervalExtensions.FromNotes(note, otherNote) switch
    {
        Interval.MinorSecond => true,
        Interval.MajorSecond => true,
        Interval.Tritone => true,
        Interval.MinorSeventh => true,
        Interval.MajorSeventh => true,
        _ => false
    };
}

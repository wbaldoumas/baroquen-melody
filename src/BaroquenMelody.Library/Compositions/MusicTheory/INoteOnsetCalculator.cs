using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using System.Collections;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

/// <summary>
///    Calculates note onsets for a given note's ornamentation type.
/// </summary>
public interface INoteOnsetCalculator
{
    /// <summary>
    ///     Calculates the note onsets for the given note and its ornamentation notes, specified at a thirty-second note resolution.
    /// </summary>
    /// <param name="ornamentationType">The ornamentation type of the note.</param>
    /// <returns>A bit array where the bit at index i is true if the note at index i is a note onset, and false otherwise.</returns>
    /// <remarks>The returned bit array will vary in size depending on the given composition's default note time span.</remarks>
    BitArray CalculateNoteOnsets(OrnamentationType ornamentationType);
}

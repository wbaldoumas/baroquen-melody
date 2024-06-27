using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules.Validators;

/// <summary>
///    Validates whether a leap resolution is valid.
/// </summary>
internal interface ILeapResolutionValidator
{
    /// <summary>
    ///     Determines whether a resolution of a leap is valid.
    /// </summary>
    /// <param name="nextToLastChord">The chord that precedes the last chord.</param>
    /// <param name="lastChord">The chord that precedes the next chord.</param>
    /// <param name="nextChord">The next chord.</param>
    /// <param name="notes">The notes of the musical scale. Used to identify the scale index of each note.</param>
    /// <returns>Whether the resolution of a leap is valid.</returns>
    bool HasValidLeapResolution(BaroquenChord nextToLastChord, BaroquenChord lastChord, BaroquenChord nextChord, IList<Note> notes);
}

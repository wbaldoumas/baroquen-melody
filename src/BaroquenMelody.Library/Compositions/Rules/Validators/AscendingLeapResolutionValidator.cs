using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules.Validators;

/// <inheritdoc cref="ILeapResolutionValidator"/>
internal sealed class AscendingLeapResolutionValidator : ILeapResolutionValidator
{
    private const int LeapThreshold = 2;

    public bool HasValidLeapResolution(BaroquenChord nextToLastChord, BaroquenChord lastChord, BaroquenChord nextChord, IList<Note> notes) =>
    !(
        from note in nextToLastChord.Notes
        let nextToLastNoteScaleIndex = notes.IndexOf(note.Raw)
        let lastNoteScaleIndex = notes.IndexOf(lastChord[note.Voice].Raw)
        where lastNoteScaleIndex > nextToLastNoteScaleIndex && lastNoteScaleIndex - nextToLastNoteScaleIndex > LeapThreshold
        let nextNoteScaleIndex = notes.IndexOf(nextChord[note.Voice].Raw)
        where nextNoteScaleIndex > lastNoteScaleIndex || lastNoteScaleIndex - nextNoteScaleIndex > LeapThreshold
        select lastNoteScaleIndex
    ).Any();
}

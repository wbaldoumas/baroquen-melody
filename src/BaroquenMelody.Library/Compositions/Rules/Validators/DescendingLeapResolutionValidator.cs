using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules.Validators;

/// <inheritdoc cref="ILeapResolutionValidator"/>
internal sealed class DescendingLeapResolutionValidator : ILeapResolutionValidator
{
    private const int LeapThreshold = 2;

    public bool HasValidLeapResolution(BaroquenChord nextToLastChord, BaroquenChord lastChord, BaroquenChord nextChord, IList<Note> notes) =>
    !(
        from note in nextToLastChord.Notes
        let nextToLastNoteScaleIndex = notes.IndexOf(note.Raw)
        let lastNoteScaleIndex = notes.IndexOf(lastChord[note.Voice].Raw)
        where lastNoteScaleIndex < nextToLastNoteScaleIndex && nextToLastNoteScaleIndex - lastNoteScaleIndex > LeapThreshold
        let nextNoteScaleIndex = notes.IndexOf(nextChord[note.Voice].Raw)
        where nextNoteScaleIndex < lastNoteScaleIndex || nextNoteScaleIndex - lastNoteScaleIndex > LeapThreshold
        select lastNoteScaleIndex
    ).Any();
}

using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="INoteChoiceGenerator"/>
internal sealed class NoteChoiceGenerator(byte minPitchChange = 1, byte maxPitchChange = 6) : INoteChoiceGenerator
{
    public ISet<NoteChoice> GenerateNoteChoices(Voice voice) => Enumerable
        .Range(minPitchChange, maxPitchChange - minPitchChange + 1)
        .Select(pitchChange => (byte)pitchChange)
        .SelectMany(pitchChange =>
            new[] { NoteMotion.Ascending, NoteMotion.Descending }.Select(noteMotion =>
                new NoteChoice(voice, noteMotion, pitchChange)
            )
        )
        .Append(new NoteChoice(voice, NoteMotion.Oblique, 0))
        .ToHashSet();
}

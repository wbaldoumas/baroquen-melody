using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="INoteChoiceGenerator"/>
internal sealed class NoteChoiceGenerator(byte minScaleStepChange = 1, byte maxScaleStepChange = 4) : INoteChoiceGenerator
{
    private readonly NoteMotion[] noteMotions = [NoteMotion.Ascending, NoteMotion.Descending];

    public ISet<NoteChoice> GenerateNoteChoices(Voice voice) => Enumerable
        .Range(minScaleStepChange, maxScaleStepChange - minScaleStepChange + 1)
        .Select(scaleStepChange => (byte)scaleStepChange)
        .SelectMany(scaleStepChange => noteMotions.Select(noteMotion => new NoteChoice(voice, noteMotion, scaleStepChange)))
        .Append(new NoteChoice(voice, NoteMotion.Oblique, 0))
        .ToHashSet();
}

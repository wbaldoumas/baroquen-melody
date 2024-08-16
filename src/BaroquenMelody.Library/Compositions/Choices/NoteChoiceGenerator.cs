using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Choices;

/// <inheritdoc cref="INoteChoiceGenerator"/>
internal sealed class NoteChoiceGenerator(byte minScaleStepChange = 1, byte maxScaleStepChange = 5) : INoteChoiceGenerator
{
    private readonly NoteMotion[] noteMotions = [NoteMotion.Ascending, NoteMotion.Descending];

    public ISet<NoteChoice> GenerateNoteChoices(Instrument instrument) => Enumerable
        .Range(minScaleStepChange, maxScaleStepChange - minScaleStepChange + 1)
        .Select(static scaleStepChange => (byte)scaleStepChange)
        .SelectMany(scaleStepChange => noteMotions.Select(noteMotion => new NoteChoice(instrument, noteMotion, scaleStepChange)))
        .Append(new NoteChoice(instrument, NoteMotion.Oblique, 0))
        .ToHashSet();
}

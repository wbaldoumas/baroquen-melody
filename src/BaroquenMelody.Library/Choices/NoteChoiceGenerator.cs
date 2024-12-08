using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Choices;

/// <inheritdoc cref="INoteChoiceGenerator"/>
internal sealed class NoteChoiceGenerator(byte minScaleStepChange = 1, byte maxScaleStepChange = CompositionConfiguration.MaxScaleStepChange) : INoteChoiceGenerator
{
    private readonly NoteMotion[] _noteMotions = [NoteMotion.Ascending, NoteMotion.Descending];

    public ISet<NoteChoice> GenerateNoteChoices(Instrument instrument) => Enumerable
        .Range(minScaleStepChange, maxScaleStepChange - minScaleStepChange + 1)
        .Select(static scaleStepChange => (byte)scaleStepChange)
        .SelectMany(scaleStepChange => _noteMotions.Select(noteMotion => new NoteChoice(instrument, noteMotion, scaleStepChange)))
        .Append(new NoteChoice(instrument, NoteMotion.Oblique, 0))
        .ToFrozenSet();
}

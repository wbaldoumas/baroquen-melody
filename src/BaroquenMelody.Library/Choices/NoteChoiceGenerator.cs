using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Choices;

/// <inheritdoc cref="INoteChoiceGenerator"/>
internal sealed class NoteChoiceGenerator(byte minScaleStepChange = 1, byte maxScaleStepChange = CompositionConfiguration.MaxScaleStepChange) : INoteChoiceGenerator
{
    private readonly NoteMotion[] _noteMotions = [NoteMotion.Ascending, NoteMotion.Descending];

    // The generation order is the canonical chord-choice index space: a hash-ordered set is not process-stable
    // (FrozenSet enumeration follows the frozen hash layout), which made seeded compositions differ between
    // processes depending on what had executed earlier.
    public IReadOnlyList<NoteChoice> GenerateNoteChoices(Instrument instrument) => Enumerable
        .Range(minScaleStepChange, maxScaleStepChange - minScaleStepChange + 1)
        .Select(static scaleStepChange => (byte)scaleStepChange)
        .SelectMany(scaleStepChange => _noteMotions.Select(noteMotion => new NoteChoice(instrument, noteMotion, scaleStepChange)))
        .Append(new NoteChoice(instrument, NoteMotion.Oblique, 0))
        .ToList();
}

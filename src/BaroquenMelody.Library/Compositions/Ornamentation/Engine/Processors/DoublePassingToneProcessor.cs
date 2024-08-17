using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

/// <inheritdoc cref="IProcessor{T}"/>>
internal sealed class DoublePassingToneProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    OrnamentationType ornamentationType
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 3;

    private readonly int[] _ornamentationTranslations = [1, 2];

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];

        var currentNoteIndex = compositionConfiguration.Scale.IndexOf(currentNote);
        var nextNoteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var isDescending = currentNoteIndex > nextNoteIndex;
        var notes = compositionConfiguration.Scale.GetNotes();

        var ornamentationNotes = _ornamentationTranslations
            .Select(ornamentationTranslation => isDescending ? currentNoteIndex - ornamentationTranslation : currentNoteIndex + ornamentationTranslation)
            .Select(index => notes[index]);

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);

        var ornamentationTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter);

        foreach (var ornamentation in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Instrument, ornamentation, ornamentationTimeSpan));
        }

        currentNote.OrnamentationType = ornamentationType;
    }
}

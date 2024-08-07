using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class RepeatedNoteProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    OrnamentationType ornamentationType
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];

        var ornamentationNote = new BaroquenNote(item.Voice, currentNote.Raw, musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter));

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);
        currentNote.Ornamentations.Add(ornamentationNote);
        currentNote.OrnamentationType = ornamentationType;
    }
}

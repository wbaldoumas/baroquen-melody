using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class RepeatedNoteProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration,
    OrnamentationType ornamentationType
) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];

        var ornamentationNote = new BaroquenNote(item.Instrument, currentNote.Raw, musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter));

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);
        currentNote.Ornamentations.Add(ornamentationNote);
        currentNote.OrnamentationType = ornamentationType;
    }
}

using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class SustainedNoteProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 0;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];

        currentNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.Sustain, compositionConfiguration.Meter);
        nextNote.MusicalTimeSpan = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.MidSustain, compositionConfiguration.Meter);

        currentNote.OrnamentationType = OrnamentationType.Sustain;
        nextNote.OrnamentationType = OrnamentationType.MidSustain;
    }
}

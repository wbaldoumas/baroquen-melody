using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors;

internal sealed class SustainedNoteProcessor(CompositionConfiguration compositionConfiguration) : IProcessor<OrnamentationItem>
{
    public const int Interval = 0;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat![item.Instrument];

        currentNote.MusicalTimeSpan = compositionConfiguration.DefaultNoteTimeSpan + nextNote.MusicalTimeSpan;

        currentNote.OrnamentationType = OrnamentationType.Sustain;
        nextNote.OrnamentationType = OrnamentationType.MidSustain;
    }
}

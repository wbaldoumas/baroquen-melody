using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

/// <inheritdoc cref="IProcessor{T}"/>
internal sealed class SustainedNoteProcessor(IMusicalTimeSpanCalculator musicalTimeSpanCalculator, CompositionConfiguration configuration) : IProcessor<OrnamentationItem>
{
    public const int Interval = 0;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.Sustain, configuration.Meter);
        nextNote.Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.MidSustain, configuration.Meter);

        currentNote.OrnamentationType = OrnamentationType.Sustain;
        nextNote.OrnamentationType = OrnamentationType.MidSustain;
    }
}

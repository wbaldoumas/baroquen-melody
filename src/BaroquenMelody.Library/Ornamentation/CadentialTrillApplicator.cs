using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Ornamentation;

/// <inheritdoc cref="ICadentialTrillApplicator"/>
/// <remarks>
///     Trills are idiomatically expected at authentic cadences, so this applies the trill deliberately rather
///     than through the probability-gated ornamentation engine. The trill lands on the highest voice sounding
///     the leading tone in the cadential dominant, and is skipped entirely when the trill ornamentation is
///     disabled, no voice sounds the leading tone, or the trill's neighbor notes fall outside the voice's range.
/// </remarks>
internal sealed class CadentialTrillApplicator(
    ICadenceClassifier cadenceClassifier,
    IOrnamentationProcessorConfigurationFactory ornamentationProcessorConfigurationFactory,
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : ICadentialTrillApplicator
{
    public void ApplyTrill(BaroquenChord penultimateChord, BaroquenChord finalChord)
    {
        var cadenceType = cadenceClassifier.ClassifyCadence(penultimateChord, finalChord);

        if (cadenceType != CadenceType.PerfectAuthentic && cadenceType != CadenceType.ImperfectAuthentic)
        {
            return;
        }

        var trillConfiguration = compositionConfiguration.AggregateOrnamentationConfiguration.Configurations
            .FirstOrDefault(static configuration => configuration.OrnamentationType == OrnamentationType.Trill);

        if (trillConfiguration is not { IsEnabled: true })
        {
            return;
        }

        if (FindLeadingToneNote(penultimateChord) is not { } leadingToneNote)
        {
            return;
        }

        var ornamentationItem = new OrnamentationItem(leadingToneNote.Instrument, [], new Beat(penultimateChord), new Beat(finalChord));

        if (!TrillNeighborsAreWithinInstrumentRange(ornamentationItem))
        {
            return;
        }

        var trillProcessorConfiguration = ornamentationProcessorConfigurationFactory.Create(trillConfiguration).First();
        var trillProcessor = new OrnamentationProcessor(musicalTimeSpanCalculator, compositionConfiguration, trillProcessorConfiguration);

        leadingToneNote.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
        trillProcessor.Process(ornamentationItem);
    }

    private BaroquenNote? FindLeadingToneNote(BaroquenChord chord)
    {
        foreach (var instrument in compositionConfiguration.Instruments)
        {
            if (chord.ContainsInstrument(instrument) && chord[instrument].NoteName == compositionConfiguration.Scale.LeadingTone)
            {
                return chord[instrument];
            }
        }

        return null;
    }

    private bool TrillNeighborsAreWithinInstrumentRange(OrnamentationItem ornamentationItem) =>
        new IsIntervalWithinInstrumentRange(compositionConfiguration, 1).ShouldProcess(ornamentationItem) == InputPolicyResult.Continue
            && new IsIntervalWithinInstrumentRange(compositionConfiguration, -1).ShouldProcess(ornamentationItem) == InputPolicyResult.Continue;
}

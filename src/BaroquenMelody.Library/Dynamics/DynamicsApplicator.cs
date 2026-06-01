using Atrea.PolicyEngine;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Dynamics;

internal sealed class DynamicsApplicator(CompositionConfiguration configuration, IPolicyEngine<DynamicsApplicationItem> dynamicsEngine) : IDynamicsApplicator
{
    private const int ContextSize = 20;

    public void Apply(Composition composition)
    {
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        var beatStrengths = composition.Measures
            .SelectMany(static measure => measure.Beats.Select((_, beatIndexInMeasure) => BeatStrengthCalculator.Calculate(beatIndexInMeasure, measure.Beats.Count)))
            .ToList();

        var processedInstruments = new HashSet<Instrument>();

        foreach (var instrument in configuration.Instruments)
        {
            Apply(instrument, processedInstruments, beats, beatStrengths);

            processedInstruments.Add(instrument);
        }
    }

    private void Apply(Instrument instrument, HashSet<Instrument> processedInstruments, List<Beat> beats, List<MetricStrength> beatStrengths)
    {
        var compositionContext = new FixedSizeList<Beat>(ContextSize);

        for (var i = 0; i < beats.Count; i++)
        {
            var currentBeat = beats[i];
            var nextBeat = beats.ElementAtOrDefault(i + 1);

            var dynamicsApplicationItem = new DynamicsApplicationItem
            {
                Instrument = instrument,
                ProcessedInstruments = processedInstruments,
                PrecedingBeats = compositionContext,
                CurrentBeat = currentBeat,
                NextBeat = nextBeat,
                CurrentBeatStrength = beatStrengths[i]
            };

            dynamicsEngine.Process(dynamicsApplicationItem);

            compositionContext.Add(beats[i]);

            if (!dynamicsApplicationItem.HasProcessedNextBeat)
            {
                continue;
            }

            i++;
            compositionContext.Add(beats[i]);
        }
    }
}

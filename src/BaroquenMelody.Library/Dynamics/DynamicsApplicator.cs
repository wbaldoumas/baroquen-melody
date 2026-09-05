using Atrea.PolicyEngine;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics.Enums;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Dynamics;

internal sealed class DynamicsApplicator(CompositionConfiguration configuration, IPolicyEngine<DynamicsApplicationItem> dynamicsEngine) : IDynamicsApplicator
{
    private const int ContextSize = 20;

    // Deterministic velocity boosts for metrically strong and medium beats, audible above the walk's one-unit steps.
    private const int StrongBeatAccent = 8;

    private const int MediumBeatAccent = 4;

    public void Apply(Composition composition)
    {
        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        var beatStrengths = composition.Measures
            .SelectMany(static measure => measure.Beats.Select((_, beatIndexInMeasure) => BeatStrengthCalculator.Calculate(beatIndexInMeasure, measure.Beats.Count)))
            .ToList();

        var processedInstruments = new HashSet<Instrument>();

        foreach (var instrument in configuration.Instruments)
        {
            Apply(instrument, processedInstruments, beats);

            processedInstruments.Add(instrument);
        }

        ApplyMetricAccents(beats, beatStrengths);
    }

    private void Apply(Instrument instrument, HashSet<Instrument> processedInstruments, List<Beat> beats)
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
                NextBeat = nextBeat
            };

            dynamicsEngine.Process(dynamicsApplicationItem);

            compositionContext.Add(beats[i]);
        }
    }

    // The accent is layered on after the velocity walk so it can never feed back into it: the walk steps from each
    // preceding note's stored velocity, so an accent applied inside the engine compounds beat over beat and pins
    // every voice at the instrument ceiling. Following the terrace/prominence precedent this pass draws nothing,
    // applies exactly once per note, clamps to the instrument's velocity window, and mirrors sub-notes.
    private void ApplyMetricAccents(List<Beat> beats, List<MetricStrength> beatStrengths)
    {
        for (var i = 0; i < beats.Count; i++)
        {
            var accent = beatStrengths[i] switch
            {
                MetricStrength.Strong => StrongBeatAccent,
                MetricStrength.Medium => MediumBeatAccent,
                _ => 0
            };

            if (accent == 0)
            {
                continue;
            }

            foreach (var instrument in configuration.Instruments)
            {
                if (!beats[i].ContainsInstrument(instrument))
                {
                    continue;
                }

                ApplyAccent(beats[i][instrument], accent);
            }
        }
    }

    private void ApplyAccent(BaroquenNote note, int accent)
    {
        var instrumentConfiguration = configuration.InstrumentConfigurationsByInstrument[note.Instrument];

        var accentedVelocity = new SevenBitNumber((byte)Math.Clamp(
            note.Velocity + accent,
            (int)instrumentConfiguration.MinVelocity,
            (int)instrumentConfiguration.MaxVelocity));

        note.Velocity = accentedVelocity;

        foreach (var ornamentation in note.Ornamentations)
        {
            ornamentation.Velocity = accentedVelocity;
        }
    }
}

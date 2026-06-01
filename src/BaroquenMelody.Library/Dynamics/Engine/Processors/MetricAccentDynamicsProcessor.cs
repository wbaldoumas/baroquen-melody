using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Dynamics.Engine.Processors;

/// <summary>
///     Boosts the velocity of metrically strong beats so that downbeats and secondary accents are heard more
///     prominently. Deterministic (no randomness); every result is clamped to the instrument's velocity range.
/// </summary>
internal sealed class MetricAccentDynamicsProcessor(
    CompositionConfiguration configuration,
    int strongBeatAccent = 8,
    int mediumBeatAccent = 4
) : IProcessor<DynamicsApplicationItem>
{
    public void Process(DynamicsApplicationItem item)
    {
        var accent = item.CurrentBeatStrength switch
        {
            MetricStrength.Strong => strongBeatAccent,
            MetricStrength.Medium => mediumBeatAccent,
            _ => 0
        };

        if (accent == 0)
        {
            return;
        }

        var instrumentConfiguration = configuration.InstrumentConfigurationsByInstrument[item.Instrument];
        var note = item.CurrentBeat[item.Instrument];

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

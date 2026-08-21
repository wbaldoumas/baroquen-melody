using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record AggregateOrnamentationConfiguration(ISet<OrnamentationConfiguration> Configurations)
{
    public static AggregateOrnamentationConfiguration Default { get; } = new(
        new HashSet<OrnamentationConfiguration>
        {
            new(OrnamentationType.PassingTone, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.DoublePassingTone, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.DelayedDoublePassingTone, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.DoubleTurn, ConfigurationStatus.Enabled, 30),
            new(OrnamentationType.DoubleInvertedTurn, ConfigurationStatus.Enabled, 30),
            new(OrnamentationType.DelayedPassingTone, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.DelayedNeighborTone, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.NeighborTone, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.Run, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.DoubleRun, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.Turn, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.InvertedTurn, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.DelayedRun, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.Mordent, ConfigurationStatus.Enabled, 20),
            new(OrnamentationType.DecorateInterval, ConfigurationStatus.Enabled, 60),
            new(OrnamentationType.Pedal, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.RepeatedNote, ConfigurationStatus.Enabled, 15),
            new(OrnamentationType.DelayedRepeatedNote, ConfigurationStatus.Enabled, 15),
            new(OrnamentationType.Pickup, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.DelayedPickup, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.DoublePickup, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.DelayedDoublePickup, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.DecorateThird, ConfigurationStatus.Enabled, 60),

            // The octave-pedal family is deliberately far below the workhorse tier: the ear reads the
            // octave bounce as a mannerism when it recurs. The static-interior pair sits at the
            // repetition-figure tier (their cell is one pitch class four times, like RepeatedNote), the
            // four moving-interior variants at the sprinkle tier beside their cousins
            // (DoublePedalPassingTone, Arpeggio).
            new(OrnamentationType.OctavePedal, ConfigurationStatus.Enabled, 15),
            new(OrnamentationType.OctavePedalPassingTone, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.OctavePedalArpeggio, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.UpperOctavePedal, ConfigurationStatus.Enabled, 15),
            new(OrnamentationType.UpperOctavePedalPassingTone, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.UpperOctavePedalArpeggio, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.TriplePickup, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.SequencedThirds, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.DoublePedalPassingTone, ConfigurationStatus.Enabled, 25),
            new(OrnamentationType.Trill, ConfigurationStatus.Enabled, 20),
            new(OrnamentationType.Appoggiatura, ConfigurationStatus.Enabled, 20),

            // Deliberately the sprinkle tier: as a stock ornament the Alberti cell is seasoning, while the
            // broken-chord texture lifts it to near-certainty at engine build regardless of this value.
            new(OrnamentationType.Arpeggio, ConfigurationStatus.Enabled, 25)
        }
    );
}

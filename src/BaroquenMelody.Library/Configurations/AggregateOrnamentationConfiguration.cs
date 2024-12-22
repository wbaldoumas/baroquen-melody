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
            new(OrnamentationType.OctavePedal, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.OctavePedalPassingTone, ConfigurationStatus.Enabled, 80),
            new(OrnamentationType.OctavePedalArpeggio, ConfigurationStatus.Enabled, 80)
        }
    );
}

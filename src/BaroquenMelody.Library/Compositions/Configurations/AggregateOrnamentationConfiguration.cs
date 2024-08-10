using System.Diagnostics.CodeAnalysis;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations;

[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record AggregateOrnamentationConfiguration(ISet<OrnamentationConfiguration> Configurations)
{
    public static AggregateOrnamentationConfiguration Default { get; } = new(
        new HashSet<OrnamentationConfiguration>
        {
            new(OrnamentationType.PassingTone, true, 80),
            new(OrnamentationType.DoublePassingTone, true, 80),
            new(OrnamentationType.DelayedDoublePassingTone, true, 80),
            new(OrnamentationType.DoubleTurn, true, 30),
            new(OrnamentationType.DelayedPassingTone, true, 80),
            new(OrnamentationType.DelayedNeighborTone, true, 25),
            new(OrnamentationType.NeighborTone, true, 25),
            new(OrnamentationType.Run, true, 80),
            new(OrnamentationType.DoubleRun, true, 15),
            new(OrnamentationType.Turn, true, 80),
            new(OrnamentationType.AlternateTurn, true, 80),
            new(OrnamentationType.DelayedRun, true, 15),
            new(OrnamentationType.Mordent, true, 1),
            new(OrnamentationType.DecorateInterval, true, 30),
            new(OrnamentationType.Pedal, true, 80),
            new(OrnamentationType.RepeatedNote, true, 5),
            new(OrnamentationType.DelayedRepeatedNote, true, 5)
        }
    );
}

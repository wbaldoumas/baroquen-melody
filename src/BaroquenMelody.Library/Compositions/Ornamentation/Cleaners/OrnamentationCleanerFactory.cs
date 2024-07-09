using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <inheritdoc cref="IOrnamentationCleanerFactory"/>
internal sealed class OrnamentationCleanerFactory : IOrnamentationCleanerFactory
{
    private readonly Lazy<IOrnamentationCleaner> _passingToneOrnamentationCleaner = new(() => new PassingToneOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _sixteenthNoteOrnamentationCleaner = new(() => new SixteenthNoteOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _passingToneSixteenthNoteOrnamentationCleaner = new(() => new PassingToneSixteenthNoteOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _turnAlternateTurnOrnamentationCleaner = new(() => new TurnAlternateTurnOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _noOpOrnamentationCleaner = new(() => new NoOpOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _thirtySecondNoteOrnamentationCleaner = new(() => new ThirtySecondNoteOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _thirtySecondSixteenthNoteOrnamentationCleaner = new(() => new ThirtySecondSixteenthNoteOrnamentationCleaner());

#pragma warning disable MA0051
    public IOrnamentationCleaner Get(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB) => (ornamentationTypeA, ornamentationTypeB) switch
    {
        (OrnamentationType.PassingTone, OrnamentationType.PassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.AlternateTurn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.Turn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.AlternateTurn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.Turn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.AlternateTurn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.DecorateInterval) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.DecorateInterval) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.DecorateInterval) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.Pedal) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.Turn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.AlternateTurn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.DecorateInterval) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.Pedal) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.Pedal) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.Pedal) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.Pedal) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.Turn) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.AlternateTurn) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.SixteenthNoteRun) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.Turn) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.AlternateTurn) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.DoublePassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.DoublePassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.DoublePassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.DecorateInterval) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.DoublePassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.Pedal) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.DoublePassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoublePassingTone, OrnamentationType.Pedal) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.AlternateTurn) => _turnAlternateTurnOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.Turn) => _turnAlternateTurnOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn) => _thirtySecondNoteOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DoubleTurn) => _thirtySecondNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondNoteOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.SixteenthNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Turn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.AlternateTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DecorateInterval) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.SixteenthNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.DoubleTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.Turn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.DoubleTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.AlternateTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.AlternateTurn, OrnamentationType.DoubleTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DecorateInterval, OrnamentationType.DoubleTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.ThirtySecondNoteRun) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Pedal) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Pedal, OrnamentationType.DoubleTurn) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.DoubleTurn, OrnamentationType.Pedal) => _thirtySecondSixteenthNoteOrnamentationCleaner.Value,
        _ => _noOpOrnamentationCleaner.Value
    };
#pragma warning restore MA0051 // Method is too long
}

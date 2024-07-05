using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;

/// <inheritdoc cref="IOrnamentationCleanerFactory"/>
internal sealed class OrnamentationCleanerFactory : IOrnamentationCleanerFactory
{
    private readonly Lazy<IOrnamentationCleaner> _passingToneOrnamentationCleaner = new(() => new PassingToneOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _sixteenthNoteOrnamentationCleaner = new(() => new SixteenthNoteOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _passingToneSixteenthNoteOrnamentationCleaner = new(() => new PassingToneSixteenthNoteOrnamentationCleaner());

    private readonly Lazy<IOrnamentationCleaner> _noOpOrnamentationCleaner = new(() => new NoOpOrnamentationCleaner());

    public IOrnamentationCleaner Get(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB) => (ornamentationTypeA, ornamentationTypeB) switch
    {
        (OrnamentationType.PassingTone, OrnamentationType.PassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone) => _passingToneOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.SixteenthNoteRun) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.Turn) => _sixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.SixteenthNoteRun, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.PassingTone, OrnamentationType.Turn) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        (OrnamentationType.Turn, OrnamentationType.PassingTone) => _passingToneSixteenthNoteOrnamentationCleaner.Value,
        _ => _noOpOrnamentationCleaner.Value
    };
}

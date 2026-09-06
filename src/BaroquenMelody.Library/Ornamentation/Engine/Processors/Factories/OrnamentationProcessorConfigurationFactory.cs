using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     Builds the processor configurations of one ornamentation type: the input policies that gate a figure, the
///     scale-step translations that spell it and how it inverts. This part holds the shared policies and the
///     dispatch; each family of figures lives in its own part (<c>OrnamentationProcessorConfigurationFactory.*.cs</c>)
///     so that a change to one family re-tests that file's mutants alone on a pull request, Stryker's
///     <c>--since</c> being file-granular.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory(
    IChordNumberIdentifier chordNumberIdentifier,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    CompositionConfiguration compositionConfiguration,
    ILogger logger
) : IOrnamentationProcessorConfigurationFactory
{
    private const int PassingToneInterval = 2;

    private static readonly IInputPolicy<OrnamentationItem> _hasNoOrnamentation = new Not<OrnamentationItem>(new HasOrnamentation());

    private static readonly IInputPolicy<OrnamentationItem> _hasNextBeat = new HasNextBeat();

    private static readonly IInputPolicy<OrnamentationItem> _isRepeatedNote = new IsRepeatedNote();

    private static readonly IInputPolicy<OrnamentationItem> _isAscending = new IsAscending();

    private static readonly IInputPolicy<OrnamentationItem> _isDescending = new IsDescending();

    private static readonly IInputPolicy<OrnamentationItem> _isNotRepeatedNote = new Not<OrnamentationItem>(new IsRepeatedNote());

    private static readonly Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldInvertBasedOnDirection =
        notes => notes.CurrentNote is not null
                 && notes.NextNote is not null
                 && notes.CurrentNote.NoteNumber > notes.NextNote.NoteNumber;

    private static readonly Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldNotInvert = _ => false;

    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = weightedRandomBooleanGenerator;

    private readonly CompositionConfiguration _compositionConfiguration = compositionConfiguration;

    private readonly ILogger _logger = logger;

    private readonly IInputPolicy<OrnamentationItem> _isRootOfChord = new IsRootOfChord(chordNumberIdentifier, compositionConfiguration);

    private readonly IInputPolicy<OrnamentationItem> _isThirdOfChord = new IsThirdOfChord(chordNumberIdentifier, compositionConfiguration);

    private readonly IInputPolicy<OrnamentationItem> _isFifthOfChord = new IsFifthOfChord(chordNumberIdentifier, compositionConfiguration);

    private Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldInvertRandomly => _ => _weightedRandomBooleanGenerator.IsTrue();

    public IEnumerable<OrnamentationProcessorConfiguration> Create(OrnamentationConfiguration ornamentationConfiguration)
    {
        var wantsToOrnament = new WantsToOrnament(_weightedRandomBooleanGenerator, ornamentationConfiguration.Probability);
        var logOrnamentation = new LogOrnamentation(ornamentationConfiguration.OrnamentationType, _logger);

        return ornamentationConfiguration.OrnamentationType switch
        {
            OrnamentationType.PassingTone => [CreatePassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Run => [CreateRun(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedPassingTone => [CreateDelayedPassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Turn => [CreateTurn(wantsToOrnament, logOrnamentation)],
            OrnamentationType.InvertedTurn => [CreateInvertedTurn(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedRun => [CreateDelayedRun(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DoubleTurn => [CreateDoubleTurn(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DoubleInvertedTurn => [CreateDoubleInvertedTurn(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DoublePassingTone => [CreateDoublePassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DecorateInterval => CreateDecorateIntervals(ornamentationConfiguration),
            OrnamentationType.DoubleRun => [CreateDoubleRun(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Pedal => CreatePedals(ornamentationConfiguration),
            OrnamentationType.Mordent => [CreateMordent(wantsToOrnament, logOrnamentation)],
            OrnamentationType.RepeatedNote => [CreateRepeatedNote(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedRepeatedNote => [CreateDelayedRepeatedNote(wantsToOrnament, logOrnamentation)],
            OrnamentationType.NeighborTone => [CreateNeighborTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedNeighborTone => [CreateDelayedNeighborTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Pickup => [CreatePickup(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedDoublePassingTone => [CreateDelayedDoublePassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedPickup => [CreateDelayedPickup(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DoublePickup => [CreateDoublePickup(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DelayedDoublePickup => [CreateDelayedDoublePickup(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DecorateThird => [CreateDecorateThird(wantsToOrnament, logOrnamentation)],
            OrnamentationType.OctavePedal => [CreateOctavePedal(wantsToOrnament, logOrnamentation)],
            OrnamentationType.OctavePedalPassingTone => [CreateOctavePedalPassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.OctavePedalArpeggio => [CreateOctavePedalArpeggio(wantsToOrnament, logOrnamentation)],
            OrnamentationType.UpperOctavePedal => [CreateUpperOctavePedal(wantsToOrnament, logOrnamentation)],
            OrnamentationType.UpperOctavePedalPassingTone => [CreateUpperOctavePedalPassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.UpperOctavePedalArpeggio => [CreateUpperOctavePedalArpeggio(wantsToOrnament, logOrnamentation)],
            OrnamentationType.TriplePickup => [CreateTriplePickup(wantsToOrnament, logOrnamentation)],
            OrnamentationType.SequencedThirds => [CreateSequencedThirds(wantsToOrnament, logOrnamentation)],
            OrnamentationType.DoublePedalPassingTone => [CreateDoublePedalPassingTone(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Trill => [CreateTrill(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Appoggiatura => [CreateAppoggiatura(wantsToOrnament, logOrnamentation)],
            OrnamentationType.Arpeggio => CreateArpeggios(ornamentationConfiguration),
            _ => throw new ArgumentOutOfRangeException(nameof(ornamentationConfiguration), ornamentationConfiguration.OrnamentationType, "Ornamentation type not supported."),
        };
    }
}

using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Random;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy : ICompositionStrategy
{
    private readonly IChordChoiceRepository _chordChoiceRepository;

    private readonly IChordContextRepository _chordContextRepository;

    private readonly IRandomTrueIndexSelector _randomTrueIndexSelector;

    private readonly IDictionary<BigInteger, BitArray> _chordContextToChordChoiceMap;

    private readonly CompositionConfiguration _compositionConfiguration;

    public CompositionStrategy(
        IChordChoiceRepository chordChoiceRepository,
        IChordContextRepository chordContextRepository,
        IRandomTrueIndexSelector randomTrueIndexSelector,
        IDictionary<BigInteger, BitArray> chordContextToChordChoiceMap,
        CompositionConfiguration compositionConfiguration)
    {
        _chordChoiceRepository = chordChoiceRepository;
        _chordContextRepository = chordContextRepository;
        _randomTrueIndexSelector = randomTrueIndexSelector;
        _chordContextToChordChoiceMap = chordContextToChordChoiceMap;
        _compositionConfiguration = compositionConfiguration;
    }

    public ChordChoice GetNextChordChoice(ChordContext chordContext)
    {
        var chordContextIndex = _chordContextRepository.GetChordContextIndex(chordContext);
        var chordChoiceIndices = _chordContextToChordChoiceMap[chordContextIndex];

        while (true)
        {
            var chordChoiceIndex = _randomTrueIndexSelector.SelectRandomTrueIndex(chordChoiceIndices);
            var chordChoice = _chordChoiceRepository.GetChordChoice(chordChoiceIndex);
            var chord = chordContext.ApplyChordChoice(chordChoice);

            if (chord.Notes.All(note => _compositionConfiguration.IsPitchInVoiceRange(note.Voice, note.Pitch)))
            {
                return chordChoice;
            }

            InvalidateChordChoice(chordContext, chordChoice);
        }
    }

    public void InvalidateChordChoice(ChordContext chordContext, ChordChoice chordChoice)
    {
        var chordContextIndex = _chordContextRepository.GetChordContextIndex(chordContext);
        var chordChoiceIndex = _chordChoiceRepository.GetChordChoiceIndex(chordChoice);

        _chordContextToChordChoiceMap[chordContextIndex][(int)chordChoiceIndex] = false;
    }
}

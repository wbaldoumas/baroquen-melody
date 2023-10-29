using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Random;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Composition.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy : ICompositionStrategy
{
    private readonly IChordChoiceRepository _chordChoiceRepository;

    private readonly IChordContextRepository _chordContextRepository;

    private readonly IRandomTrueIndexSelector _randomTrueIndexSelector;

    private readonly IDictionary<BigInteger, BitArray> _chordContextToChordChoiceMap;

    public CompositionStrategy(
        IChordChoiceRepository chordChoiceRepository,
        IChordContextRepository chordContextRepository,
        IRandomTrueIndexSelector randomTrueIndexSelector,
        IDictionary<BigInteger, BitArray> chordContextToChordChoiceMap)
    {
        _chordChoiceRepository = chordChoiceRepository;
        _chordContextRepository = chordContextRepository;
        _randomTrueIndexSelector = randomTrueIndexSelector;
        _chordContextToChordChoiceMap = chordContextToChordChoiceMap;
    }

    public ChordChoice GetNextChordChoice(ChordContext chordContext)
    {
        var chordContextIndex = _chordContextRepository.GetChordContextIndex(chordContext);
        var chordChoiceIndices = _chordContextToChordChoiceMap[chordContextIndex];
        var chordChoiceIndex = _randomTrueIndexSelector.SelectRandomTrueIndex(chordChoiceIndices);

        return _chordChoiceRepository.GetChordChoice(chordChoiceIndex);
    }

    public void InvalidateChordChoice(ChordContext chordContext, ChordChoice chordChoice)
    {
        var chordContextIndex = _chordContextRepository.GetChordContextIndex(chordContext);
        var chordChoiceIndex = _chordChoiceRepository.GetChordChoiceIndex(chordChoice);

        _chordContextToChordChoiceMap[chordContextIndex][(int)chordChoiceIndex] = false;
    }
}

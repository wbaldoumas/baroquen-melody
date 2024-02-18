using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Random;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Strategies;

internal sealed class CompositionStrategyFactory(
    IChordChoiceRepositoryFactory chordChoiceRepositoryFactory,
    IChordContextRepositoryFactory chordContextRepositoryFactory,
    IRandomTrueIndexSelector randomTrueIndexSelector)
    : ICompositionStrategyFactory
{
    public ICompositionStrategy Create(CompositionConfiguration compositionConfiguration)
    {
        var chordChoiceRepository = chordChoiceRepositoryFactory.Create(compositionConfiguration);
        var chordContextRepository = chordContextRepositoryFactory.Create(compositionConfiguration);

        var chordContextToChordChoiceMap = CreateChordContextToChordChoiceMap(
            chordChoiceRepository.Count,
            chordContextRepository.Count
        );

        return new CompositionStrategy(
            chordChoiceRepository,
            chordContextRepository,
            randomTrueIndexSelector,
            chordContextToChordChoiceMap,
            compositionConfiguration
        );
    }

    private static CompressedBitArrayDictionary CreateChordContextToChordChoiceMap(
        BigInteger chordChoiceCount,
        BigInteger chordContextCount)
    {
        var chordContextToChordChoiceMap = new CompressedBitArrayDictionary();

        for (BigInteger chordContextIndex = 0; chordContextIndex < chordContextCount; chordContextIndex++)
        {
            var chordChoiceValues = new BitArray((int)chordChoiceCount, defaultValue: true);

            chordContextToChordChoiceMap.Add(chordContextIndex, chordChoiceValues);
        }

        return chordContextToChordChoiceMap;
    }
}

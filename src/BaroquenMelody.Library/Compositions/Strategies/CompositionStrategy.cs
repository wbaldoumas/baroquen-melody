using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Random;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <inheritdoc cref="ICompositionStrategy"/>
internal sealed class CompositionStrategy(
    IChordChoiceRepository chordChoiceRepository,
    IChordContextRepository chordContextRepository,
    IRandomTrueIndexSelector randomTrueIndexSelector,
    IDictionary<BigInteger, BitArray> chordContextToChordChoiceMap,
    CompositionConfiguration compositionConfiguration)
    : ICompositionStrategy
{
    public ChordChoice GetNextChordChoice(ChordContext chordContext)
    {
        var chordContextIndex = chordContextRepository.GetChordContextIndex(chordContext);
        var chordChoiceIndices = chordContextToChordChoiceMap[chordContextIndex];

        while (true)
        {
            var chordChoiceIndex = randomTrueIndexSelector.SelectRandomTrueIndex(chordChoiceIndices);
            var chordChoice = chordChoiceRepository.GetChordChoice(chordChoiceIndex);
            var chord = chordContext.ApplyChordChoice(chordChoice, compositionConfiguration.Scale);

            if (chord.VoicedNotes.All(voicedNote => compositionConfiguration.IsNoteInVoiceRange(voicedNote.Voice, voicedNote.Note)))
            {
                return chordChoice;
            }

            InvalidateChordChoice(chordContext, chordChoice);
        }
    }

    public void InvalidateChordChoice(ChordContext chordContext, ChordChoice chordChoice)
    {
        var chordContextIndex = chordContextRepository.GetChordContextIndex(chordContext);
        var chordChoiceIndex = chordChoiceRepository.GetChordChoiceIndex(chordChoice);

        chordContextToChordChoiceMap[chordContextIndex][(int)chordChoiceIndex] = false;
    }
}

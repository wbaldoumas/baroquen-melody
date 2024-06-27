using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Phrasing;

internal sealed class CompositionPhraser(ICompositionRule compositionRule, CompositionConfiguration compositionConfiguration) : ICompositionPhraser
{
    private const int OneHundred = 100;

    private readonly List<RepeatedPhrase> phrasesToRepeat = [];

    private readonly Queue<RepeatedPhrase> coolOffPhrases = new();

    public void AttemptPhraseRepetition(List<Measure> measures)
    {
        if (AttemptToRepeatExistingPhrase(measures))
        {
            return;
        }

        AttemptToCreateAndRepeatNewPhrase(measures);
    }

    private bool AttemptToRepeatExistingPhrase(List<Measure> measures)
    {
        var minPhraseRepetitionPoolSize = compositionConfiguration.PhrasingConfiguration.MinPhraseRepetitionPoolSize;

        if (phrasesToRepeat.Count < minPhraseRepetitionPoolSize)
        {
            return false;
        }

        foreach (var repeatedPhrase in phrasesToRepeat.OrderBy(_ => ThreadLocalRandom.Next()).ToList().Where(repeatedPhrase => TryRepeatPhrase(measures, repeatedPhrase)))
        {
            phrasesToRepeat.Remove(repeatedPhrase);

            if (coolOffPhrases.Count >= minPhraseRepetitionPoolSize && coolOffPhrases.TryDequeue(out var coolOffPhrase))
            {
                phrasesToRepeat.Add(coolOffPhrase);
            }

            // If the phrase has not reached the maximum number of repetitions, add it to the cool off queue.
            if (repeatedPhrase.RepetitionCount < compositionConfiguration.PhrasingConfiguration.MaxPhraseRepetitions)
            {
                coolOffPhrases.Enqueue(repeatedPhrase);
            }

            return true;
        }

        return false;
    }

    private void AttemptToCreateAndRepeatNewPhrase(List<Measure> measures)
    {
        foreach (var phraseLength in compositionConfiguration.PhrasingConfiguration.PhraseLengths.OrderByDescending(phraseLength => phraseLength))
        {
            if (measures.Count < phraseLength)
            {
                continue;
            }

            var lastMeasures = measures.Skip(measures.Count - phraseLength).Take(phraseLength).ToList();

            if (ThreadLocalRandom.Next(OneHundred) > compositionConfiguration.PhrasingConfiguration.PhraseRepetitionProbability || !CanRepeatPhrase(measures, lastMeasures))
            {
                continue;
            }

            var repeatedPhrase = new RepeatedPhrase
            {
                Phrase = lastMeasures,
                RepetitionCount = 1
            };

            phrasesToRepeat.Add(repeatedPhrase);
            measures.AddRange(lastMeasures);

            return;
        }
    }

    private bool TryRepeatPhrase(List<Measure> measures, RepeatedPhrase repeatedPhrase)
    {
        if (repeatedPhrase.RepetitionCount >= compositionConfiguration.PhrasingConfiguration.MaxPhraseRepetitions ||
            ThreadLocalRandom.Next(OneHundred) > compositionConfiguration.PhrasingConfiguration.PhraseRepetitionProbability ||
            !CanRepeatPhrase(measures, repeatedPhrase.Phrase))
        {
            return false;
        }

        // Clear the ornamentations to ensure smooth transitions between repetitions.
        foreach (var note in measures[^1].Beats[^1].Chord.Notes)
        {
            note.Ornamentations.Clear();
        }

        measures.AddRange(repeatedPhrase.Phrase);
        repeatedPhrase.RepetitionCount++;

        return true;
    }

    private bool CanRepeatPhrase(IEnumerable<Measure> measures, IEnumerable<Measure> measuresToRepeat)
    {
        var compositionContext = new FixedSizeList<BaroquenChord>(compositionConfiguration.CompositionContextSize);

        foreach (var measure in measures)
        {
            foreach (var beat in measure.Beats)
            {
                compositionContext.Add(beat.Chord);
            }
        }

        var firstChordOfRepeatedPhrase = measuresToRepeat.First().Beats.First().Chord;

        return compositionRule.Evaluate(compositionContext, firstChordOfRepeatedPhrase);
    }
}

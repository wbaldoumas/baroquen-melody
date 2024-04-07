using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Phrasing;

internal sealed class CompositionPhraser(ICompositionRule compositionRule, CompositionConfiguration compositionConfiguration) : ICompositionPhraser
{
    private readonly List<RepeatedPhrase> repeatedPhrases = [];

    private RepeatedPhrase? coolOffPhrase;

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
        if (repeatedPhrases.Count < compositionConfiguration.PhrasingConfiguration.MinPhraseRepetitionPoolSize)
        {
            return false;
        }

        foreach (var repeatedPhrase in repeatedPhrases.OrderBy(_ => ThreadLocalRandom.Next()).ToList().Where(repeatedPhrase => TryRepeatPhrase(measures, repeatedPhrase)))
        {
            repeatedPhrases.Remove(repeatedPhrase);

            if (coolOffPhrase is not null)
            {
                repeatedPhrases.Add(coolOffPhrase);
            }

            coolOffPhrase = repeatedPhrase;

            return true;
        }

        return false;
    }

    private void AttemptToCreateAndRepeatNewPhrase(List<Measure> measures)
    {
        foreach (var phraseLength in compositionConfiguration.PhrasingConfiguration.PhraseLengths.OrderBy(_ => ThreadLocalRandom.Next()))
        {
            if (measures.Count < phraseLength)
            {
                continue;
            }

            var lastMeasures = measures.Skip(measures.Count - phraseLength).Take(phraseLength).ToList();

            if (ThreadLocalRandom.Next(100) > compositionConfiguration.PhrasingConfiguration.PhraseRepetitionProbability || !CanRepeatPhrase(measures, lastMeasures))
            {
                continue;
            }

            var repeatedPhrase = new RepeatedPhrase
            {
                Phrase = lastMeasures,
                RepetitionCount = 1
            };

            repeatedPhrases.Add(repeatedPhrase);
            measures.AddRange(lastMeasures);

            return;
        }
    }

    private bool TryRepeatPhrase(List<Measure> measures, RepeatedPhrase repeatedPhrase)
    {
        if (repeatedPhrase.RepetitionCount >= compositionConfiguration.PhrasingConfiguration.MaxPhraseRepetitions ||
            ThreadLocalRandom.Next(100) > compositionConfiguration.PhrasingConfiguration.PhraseRepetitionProbability ||
            !CanRepeatPhrase(measures, repeatedPhrase.Phrase))
        {
            return false;
        }

        Console.WriteLine($"REPEATING PREVIOUSLY REPEATED PHRASE: {repeatedPhrase.Id}. Repeat count: {repeatedPhrase.RepetitionCount}.");

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

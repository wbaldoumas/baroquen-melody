using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Phrasing;

/// <inheritdoc cref="ICompositionPhraser"/>
internal sealed class CompositionPhraser(
    ICompositionRule compositionRule,
    IThemeSplitter themeSplitter,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    ILogger logger,
    CompositionConfiguration compositionConfiguration
) : ICompositionPhraser
{
    private readonly List<RepeatedPhrase> phrasesToRepeat = [];

    private readonly Queue<RepeatedPhrase> coolOffPhrases = new();

    private List<RepeatedPhrase> themePhrasesToRepeat = [];

    private RepeatedPhrase? themeCoolOffPhrase;

    public void AttemptPhraseRepetition(List<Measure> measures)
    {
        if (AttemptToRepeatExistingThemePhrase(measures) || AttemptToRepeatExistingPhrase(measures))
        {
            return;
        }

        AttemptToCreateAndRepeatNewPhrase(measures);
    }

    public void AddTheme(BaroquenTheme theme)
    {
        themePhrasesToRepeat = themeSplitter.SplitThemeIntoPhrases(theme);

        foreach (var themePhraseToRepeat in themePhrasesToRepeat.ToList())
        {
            ResetPhraseEndOrnamentation(themePhraseToRepeat.Phrase[^1], compositionConfiguration.DefaultNoteTimeSpan);
        }
    }

    private bool AttemptToRepeatExistingThemePhrase(List<Measure> measures)
    {
        foreach (var themePhraseToRepeat in themePhrasesToRepeat.Where(themePhraseToRepeat => themePhraseToRepeat != themeCoolOffPhrase).OrderBy(static _ => ThreadLocalRandom.Next()))
        {
            if (!TryRepeatPhrase(measures, themePhraseToRepeat))
            {
                continue;
            }

            themeCoolOffPhrase = themePhraseToRepeat;

            logger.RepeatedThemePhrase();

            return true;
        }

        return false;
    }

    private bool AttemptToRepeatExistingPhrase(List<Measure> measures)
    {
        var minPhraseRepetitionPoolSize = compositionConfiguration.PhrasingConfiguration.MinPhraseRepetitionPoolSize;

        if (phrasesToRepeat.Count < minPhraseRepetitionPoolSize)
        {
            return false;
        }

        foreach (var repeatedPhrase in phrasesToRepeat.OrderBy(static _ => ThreadLocalRandom.Next()).Where(repeatedPhrase => TryRepeatPhrase(measures, repeatedPhrase)))
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

            logger.RepeatedNonThemePhrase();

            return true;
        }

        return false;
    }

    private void AttemptToCreateAndRepeatNewPhrase(List<Measure> measures)
    {
        foreach (var phraseLength in compositionConfiguration.PhrasingConfiguration.PhraseLengths.OrderByDescending(static phraseLength => phraseLength))
        {
            if (measures.Count < phraseLength)
            {
                continue;
            }

            var lastMeasures = measures.Skip(measures.Count - phraseLength).Take(phraseLength).Select(static measure => new Measure(measure)).ToList();

            if (!weightedRandomBooleanGenerator.IsTrue(compositionConfiguration.PhrasingConfiguration.PhraseRepetitionProbability) || !CanRepeatPhrase(measures, lastMeasures))
            {
                continue;
            }

            var repeatedPhrase = new RepeatedPhrase
            {
                Phrase = lastMeasures,
                RepetitionCount = 1
            };

            phrasesToRepeat.Add(repeatedPhrase);

            ResetPhraseEndOrnamentation(measures[^1], compositionConfiguration.DefaultNoteTimeSpan);

            measures.AddRange(lastMeasures.Select(static measure => new Measure(measure)));

            return;
        }
    }

    /// <summary>
    ///     Resets the ornamentation on the last note of the last measure in the phrase if it is not a rest.
    /// </summary>
    /// <param name="measure">The measure to reset the ornamentation on.</param>
    /// <param name="defaultTimeSpan">The default time span to reset the ornamentation to.</param>
    private static void ResetPhraseEndOrnamentation(Measure measure, MusicalTimeSpan defaultTimeSpan)
    {
        foreach (var note in measure.Beats[^1].Chord.Notes.Where(static note => note.OrnamentationType != OrnamentationType.MidSustain))
        {
            note.ResetOrnamentation(defaultTimeSpan);
        }
    }

    private bool TryRepeatPhrase(List<Measure> measures, RepeatedPhrase repeatedPhrase)
    {
        if (repeatedPhrase.RepetitionCount >= compositionConfiguration.PhrasingConfiguration.MaxPhraseRepetitions ||
            !weightedRandomBooleanGenerator.IsTrue(compositionConfiguration.PhrasingConfiguration.PhraseRepetitionProbability) ||
            !CanRepeatPhrase(measures, repeatedPhrase.Phrase))
        {
            return false;
        }

        ResetPhraseEndOrnamentation(measures[^1], compositionConfiguration.DefaultNoteTimeSpan);

        measures.AddRange(repeatedPhrase.Phrase.Select(static measure => new Measure(measure)).ToList());
        repeatedPhrase.RepetitionCount++;

        return true;
    }

    private bool CanRepeatPhrase(List<Measure> measures, List<Measure> measuresToRepeat)
    {
        var compositionContext = new FixedSizeList<BaroquenChord>(compositionConfiguration.CompositionContextSize);

        foreach (var beat in measures.SelectMany(static measure => measure.Beats))
        {
            compositionContext.Add(beat.Chord);
        }

        var firstChordOfRepeatedPhrase = measuresToRepeat[0].Beats[0].Chord;

        return compositionRule.Evaluate(compositionContext, firstChordOfRepeatedPhrase);
    }
}

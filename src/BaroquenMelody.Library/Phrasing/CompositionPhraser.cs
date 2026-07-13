using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Rules;
using Melanchall.DryWetMidi.Interaction;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Phrasing;

/// <inheritdoc cref="ICompositionPhraser"/>
internal sealed class CompositionPhraser(
    ICompositionRule compositionRule,
    IThemeSplitter themeSplitter,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IRandomProvider randomProvider,
    ILogger logger,
    CompositionConfiguration compositionConfiguration,
    IMotifBankFactory motifBankFactory,
    IMotifDeveloper motifDeveloper,
    ICadentialTrillApplicator cadentialTrillApplicator
) : ICompositionPhraser
{
    private readonly List<RepeatedPhrase> _phrasesToRepeat = [];

    private readonly Queue<RepeatedPhrase> _coolOffPhrases = new();

    private List<RepeatedPhrase> _themePhrasesToRepeat = [];

    private RepeatedPhrase? _themeCoolOffPhrase;

    private MotifBank? _motifBank;

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
        _motifBank = motifBankFactory.Create(theme);

        _themePhrasesToRepeat = themeSplitter.SplitThemeIntoPhrases(theme);

        foreach (var themePhraseToRepeat in _themePhrasesToRepeat.ToList())
        {
            ResetPhraseEndOrnamentation(themePhraseToRepeat.Phrase[^1], compositionConfiguration.DefaultNoteTimeSpan);
        }
    }

    private bool AttemptToRepeatExistingThemePhrase(List<Measure> measures)
    {
        foreach (var themePhraseToRepeat in _themePhrasesToRepeat.Where(themePhraseToRepeat => themePhraseToRepeat != _themeCoolOffPhrase).OrderByRandom(randomProvider))
        {
            if (!TryRepeatPhrase(measures, themePhraseToRepeat))
            {
                continue;
            }

            _themeCoolOffPhrase = themePhraseToRepeat;

            logger.LogInfoMessage("Repeated main theme phrase.");

            return true;
        }

        return false;
    }

    private bool AttemptToRepeatExistingPhrase(List<Measure> measures)
    {
        var minPhraseRepetitionPoolSize = compositionConfiguration.PhrasingConfiguration.MinPhraseRepetitionPoolSize;

        if (_phrasesToRepeat.Count < minPhraseRepetitionPoolSize)
        {
            return false;
        }

        foreach (var repeatedPhrase in _phrasesToRepeat.OrderByRandom(randomProvider).Where(repeatedPhrase => TryRepeatPhrase(measures, repeatedPhrase)))
        {
            _phrasesToRepeat.Remove(repeatedPhrase);

            if (_coolOffPhrases.Count >= minPhraseRepetitionPoolSize && _coolOffPhrases.TryDequeue(out var coolOffPhrase))
            {
                _phrasesToRepeat.Add(coolOffPhrase);
            }

            // If the phrase has not reached the maximum number of repetitions, add it to the cool off queue.
            if (repeatedPhrase.RepetitionCount < compositionConfiguration.PhrasingConfiguration.MaxPhraseRepetitions)
            {
                _coolOffPhrases.Enqueue(repeatedPhrase);
            }

            logger.LogInfoMessage("Repeated non-theme phrase.");

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

            _phrasesToRepeat.Add(repeatedPhrase);

            FinalizeLivePhraseSeam(measures[^1]);

            measures.AddRange(lastMeasures.Select(static measure => new Measure(measure)));

            return;
        }
    }

    /// <summary>
    ///     Finalizes the seam measure of the live composition before a restatement is appended: ornamentation on the
    ///     seam's final beat is reset so the phrase can breathe, and when the seam's last two chords form an authentic
    ///     cadence, the leading-tone voice receives the idiomatic cadential trill. Stored theme phrases only get the
    ///     plain reset when they are banked in <see cref="AddTheme"/>, since their seams are not part of the live
    ///     composition. A seam ending in a held beat can never trill: the held copy duplicates the preceding harmony,
    ///     which the cadence classifier never treats as a cadence.
    /// </summary>
    /// <param name="measure">The final measure of the live composition, whose end is the phrase seam.</param>
    private void FinalizeLivePhraseSeam(Measure measure)
    {
        ResetPhraseEndOrnamentation(measure, compositionConfiguration.DefaultNoteTimeSpan);

        if (measure.Beats.Count >= 2)
        {
            cadentialTrillApplicator.ApplyTrill(measure.Beats[^2].Chord, measure.Beats[^1].Chord);
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

        FinalizeLivePhraseSeam(measures[^1]);

        measures.AddRange(BuildRestatement(measures, repeatedPhrase.Phrase));
        repeatedPhrase.RepetitionCount++;

        return true;
    }

    /// <summary>
    ///     Builds the measures to append for a restatement: a developed (transformed) restatement when motivic development
    ///     produces a candidate that passes the rule gate, otherwise a verbatim copy of the phrase.
    /// </summary>
    /// <param name="measures">The composition so far, used as the rule-evaluation context.</param>
    /// <param name="phrase">The phrase being restated.</param>
    /// <returns>The measures to append.</returns>
    private List<Measure> BuildRestatement(List<Measure> measures, List<Measure> phrase)
    {
        if (_motifBank is not null)
        {
            var developedPhrase = motifDeveloper.TryDevelop(_motifBank, phrase);

            if (developedPhrase is not null && CanAppendDevelopedPhrase(measures, developedPhrase))
            {
                logger.LogInfoMessage("Substituted a developed motivic restatement.");

                return developedPhrase;
            }
        }

        return phrase.Select(static measure => new Measure(measure)).ToList();
    }

    /// <summary>
    ///     Validates a developed restatement chord-by-chord against the same rules that govern body composition, so a
    ///     developed candidate can only be appended if it is as valid as a verbatim repetition would be.
    /// </summary>
    /// <param name="measures">The composition so far, seeding the rule-evaluation context.</param>
    /// <param name="developedPhrase">The candidate developed restatement.</param>
    /// <returns>Whether every chord of the developed restatement passes the composition rules.</returns>
    private bool CanAppendDevelopedPhrase(List<Measure> measures, List<Measure> developedPhrase)
    {
        var compositionContext = new FixedSizeList<BaroquenChord>(compositionConfiguration.CompositionContextSize);

        foreach (var chord in measures.SelectMany(static measure => measure.Beats).Select(static beat => beat.Chord))
        {
            compositionContext.Add(chord);
        }

        foreach (var chord in developedPhrase.SelectMany(static measure => measure.Beats).Select(static beat => beat.Chord))
        {
            if (!compositionRule.Evaluate(compositionContext, chord))
            {
                return false;
            }

            compositionContext.Add(chord);
        }

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

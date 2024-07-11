using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can generate a <see cref="Composition"/>.
/// </summary>
/// <param name="compositionStrategy"> The strategy that the composer should use to generate the composition. </param>
/// <param name="compositionDecorator"> The decorator that the composer should use to decorate the composition. </param>
/// <param name="compositionPhraser"> The phraser that the composer should use to phrase the composition. </param>
/// <param name="noteTransposer"> The transposer that the composer should use to transpose notes. </param>
/// <param name="compositionConfiguration"> The configuration to use to generate the composition. </param>
internal sealed class Composer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    ICompositionPhraser compositionPhraser,
    INoteTransposer noteTransposer,
    CompositionConfiguration compositionConfiguration
) : IComposer
{
    public Composition Compose()
    {
        List<Measure> openingMeasures;

        while (true)
        {
            try
            {
                openingMeasures = ComposeFugueSubject();
                break;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to compose suitable fugue subject. Retrying...");
            }
        }

        var compositionContext = new FixedSizeList<BaroquenChord>(
            compositionConfiguration.CompositionContextSize,
            openingMeasures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord))
        );

        var continuationMeasures = new List<Measure>();

        while (continuationMeasures.Count < compositionConfiguration.CompositionLength)
        {
            var initialChord = ComposeNextChord(compositionContext);
            var beats = new List<Beat>(compositionConfiguration.Meter.BeatsPerMeasure()) { new(initialChord) };

            compositionContext.Add(initialChord);

            while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
            {
                var nextChord = ComposeNextChord(compositionContext);

                compositionContext.Add(nextChord);
                beats.Add(new Beat(nextChord));
            }

            continuationMeasures.Add(new Measure(beats, compositionConfiguration.Meter));
        }

        var continuationComposition = new Composition(continuationMeasures);

        compositionDecorator.Decorate(continuationComposition);

        var phrasedComposition = PhraseComposition(continuationComposition);

        return new Composition([.. openingMeasures, .. phrasedComposition.Measures]);
    }

    private Composition PhraseComposition(Composition initialComposition)
    {
        var currentMeasureIndex = 0;
        var phrasedMeasures = new List<Measure>();

        foreach (var measure in initialComposition.Measures)
        {
            phrasedMeasures.Add(measure);

            if (currentMeasureIndex++ % compositionConfiguration.PhrasingConfiguration.PhraseLengths.Min() == 0)
            {
                compositionPhraser.AttemptPhraseRepetition(phrasedMeasures);
            }

            if (phrasedMeasures.Count >= compositionConfiguration.CompositionLength)
            {
                break;
            }
        }

        var phrasedComposition = new Composition(phrasedMeasures);

        compositionDecorator.Decorate(phrasedComposition);
        compositionDecorator.ApplySustain(phrasedComposition);

        return phrasedComposition;
    }

    private List<Measure> ComposeInitialMeasures()
    {
        var initialChord = compositionStrategy.GenerateInitialChord();
        var beats = new List<Beat>(compositionConfiguration.Meter.BeatsPerMeasure()) { new(initialChord) };
        var precedingChords = beats.Select(beat => beat.Chord).ToList();

        while (beats.Count < compositionConfiguration.Meter.BeatsPerMeasure())
        {
            var nextChord = ComposeNextChord(precedingChords);

            precedingChords.Add(nextChord);
            beats.Add(new Beat(nextChord));
        }

        return new List<Measure>(compositionConfiguration.CompositionLength)
        {
            new(beats, compositionConfiguration.Meter)
        };
    }

    private List<Measure> ComposeFugueSubject()
    {
        var initialMeasures = ComposeInitialMeasures();
        var initialComposition = new Composition(initialMeasures);

        var voices = compositionConfiguration.VoiceConfigurations
            .OrderByDescending(voiceConfiguration => voiceConfiguration.MinNote)
            .Select(voiceConfiguration => voiceConfiguration.Voice)
            .ToList();

        var fugueSubjectVoice = voices[0];

        compositionDecorator.Decorate(initialComposition, fugueSubjectVoice);

        var fugueSubject = initialComposition.Measures
            .SelectMany(measure => measure.Beats)
            .Select(beat => beat.Chord[fugueSubjectVoice])
            .ToList();

        var workingChords = initialComposition.Measures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord)).ToList();

        ContinueFugueSubject(fugueSubject, fugueSubjectVoice, workingChords, voices);

        return StripVoicesFromFugueSubject(workingChords, voices);
    }

    private void ContinueFugueSubject(List<BaroquenNote> fugueSubject, Voice fugueSubjectVoice, List<BaroquenChord> workingChords, List<Voice> voices)
    {
        var processedVoices = new List<Voice> { fugueSubjectVoice };

        foreach (var voice in voices.Where(voice => voice != fugueSubjectVoice))
        {
            var precedingChord = workingChords[^1];
            var nextChords = new List<BaroquenChord>();

            var transposedSubjectChords = noteTransposer.TransposeToVoice(fugueSubject, fugueSubjectVoice, voice)
                .Select(note => new BaroquenChord([note]))
                .ToList();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var transposedSubjectChord in transposedSubjectChords)
            {
                var nextChord = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords([precedingChord], transposedSubjectChord).OrderBy(_ => ThreadLocalRandom.Next()).First();

                var transposedSubjectNote = transposedSubjectChord[voice];
                var otherNotes = nextChord.Notes.Where(note => note.Voice != voice).ToList();
                var workingChord = new BaroquenChord([.. otherNotes, transposedSubjectNote]);

                nextChords.Add(workingChord);
                precedingChord = workingChord;
            }

            var tempComposition = new Composition([new Measure(nextChords.Select(chord => new Beat(chord)).ToList(), compositionConfiguration.Meter)]);

            foreach (var processedVoice in processedVoices)
            {
                compositionDecorator.Decorate(tempComposition, processedVoice);
            }

            workingChords.AddRange(nextChords);
            processedVoices.Add(voice);
        }
    }

    private List<Measure> StripVoicesFromFugueSubject(List<BaroquenChord> workingChords, List<Voice> voices)
    {
        var beatIndex = 0;
        var measures = new List<Measure>();
        var inProcessVoices = new List<Voice>();

        foreach (var voice in voices)
        {
            inProcessVoices.Add(voice);

            var beats = workingChords.Skip(beatIndex).Take(compositionConfiguration.Meter.BeatsPerMeasure()).Select(chord => new Beat(chord)).ToList();
            var strippedBeats = beats.Select(beat => new BaroquenChord(beat.Chord.Notes.Where(note => inProcessVoices.Contains(note.Voice)).ToList())).Select(newChord => new Beat(newChord)).ToList();

            beatIndex += compositionConfiguration.Meter.BeatsPerMeasure();

            measures.Add(new Measure(strippedBeats, compositionConfiguration.Meter));
        }

        return measures;
    }

    private BaroquenChord ComposeNextChord(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(precedingChords);
        var chordChoice = possibleChordChoices.OrderBy(_ => ThreadLocalRandom.Next()).First();

        return precedingChords[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);
    }
}

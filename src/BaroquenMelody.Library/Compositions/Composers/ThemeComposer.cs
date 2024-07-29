using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Random;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <inheritdoc cref="IThemeComposer"/>
internal sealed class ThemeComposer(
    ICompositionStrategy compositionStrategy,
    ICompositionDecorator compositionDecorator,
    IChordComposer chordComposer,
    INoteTransposer noteTransposer,
    ILogger logger,
    CompositionConfiguration compositionConfiguration
) : IThemeComposer
{
    private const int MaxFugueCompositionAttempts = 50;

    public BaroquenTheme Compose()
    {
        var attempt = 0;

        while (attempt++ < MaxFugueCompositionAttempts)
        {
            if (TryComposeFugalTheme(out var fugueSubject))
            {
                return fugueSubject!;
            }

            logger.FailedToComposeFugalThemeAttempt(attempt, MaxFugueCompositionAttempts);
        }

        logger.FailedToComposeFugalTheme(MaxFugueCompositionAttempts);

        var initialMeasures = ComposeInitialMeasures();

        return new BaroquenTheme(initialMeasures, initialMeasures);
    }

    private bool TryComposeFugalTheme(out BaroquenTheme? theme)
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

        workingChords = ContinueFugueSubject(fugueSubject, fugueSubjectVoice, workingChords, voices);

        if (workingChords.Count == 0)
        {
            theme = null;
            return false;
        }

        theme = StripVoicesFromFugueSubject(workingChords, voices);

        return true;
    }

    private List<Measure> ComposeInitialMeasures()
    {
        var initialChord = compositionStrategy.GenerateInitialChord();
        var beats = new List<Beat>(compositionConfiguration.BeatsPerMeasure) { new(initialChord) };
        var precedingChords = beats.Select(beat => beat.Chord).ToList();

        while (beats.Count < compositionConfiguration.BeatsPerMeasure)
        {
            var nextChord = chordComposer.Compose(precedingChords);

            precedingChords.Add(nextChord);
            beats.Add(new Beat(nextChord));
        }

        return new List<Measure>(compositionConfiguration.CompositionLength)
        {
            new(beats, compositionConfiguration.Meter)
        };
    }

    private List<BaroquenChord> ContinueFugueSubject(List<BaroquenNote> fugueSubject, Voice fugueSubjectVoice, List<BaroquenChord> workingChords, List<Voice> voices)
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
                var possibleChords = compositionStrategy.GetPossibleChordsForPartiallyVoicedChords([precedingChord], transposedSubjectChord);

                if (possibleChords.Count == 0)
                {
                    return [];
                }

                var nextChord = possibleChords.OrderBy(_ => ThreadLocalRandom.Next()).First();
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

            workingChords.AddRange(tempComposition.Measures.SelectMany(measure => measure.Beats.Select(beat => beat.Chord)));
            processedVoices.Add(voice);
        }

        return workingChords;
    }

    private BaroquenTheme StripVoicesFromFugueSubject(List<BaroquenChord> workingChords, List<Voice> voices)
    {
        var beatIndex = 0;
        var expositionMeasures = new List<Measure>();
        var recapitulationMeasures = new List<Measure>();
        var inProcessVoices = new List<Voice>();

        foreach (var voice in voices)
        {
            inProcessVoices.Add(voice);

            var beats = workingChords.Skip(beatIndex).Take(compositionConfiguration.BeatsPerMeasure).Select(chord => new Beat(chord)).ToList();
            var strippedBeats = beats.Select(beat => new BaroquenChord(beat.Chord.Notes.Where(note => inProcessVoices.Contains(note.Voice)).ToList())).Select(newChord => new Beat(newChord)).ToList();

            beatIndex += compositionConfiguration.BeatsPerMeasure;

            recapitulationMeasures.Add(new Measure(beats, compositionConfiguration.Meter));
            expositionMeasures.Add(new Measure(strippedBeats, compositionConfiguration.Meter));
        }

        return new BaroquenTheme(expositionMeasures, recapitulationMeasures);
    }
}

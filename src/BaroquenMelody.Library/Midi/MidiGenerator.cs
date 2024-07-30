using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Midi;

/// <inheritdoc cref="IMidiGenerator"/>
internal sealed class MidiGenerator(CompositionConfiguration compositionConfiguration) : IMidiGenerator
{
    public MidiFile Generate(Composition composition)
    {
        var patternBuildersByVoice = InitializePatternBuildersByVoice();

        foreach (var measure in composition.Measures)
        {
            ProcessMeasure(measure, patternBuildersByVoice);
        }

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(compositionConfiguration.Tempo));
        var chunks = patternBuildersByVoice.Values.Select(patternBuilder => patternBuilder.Build().ToTrackChunk(tempoMap, new FourBitNumber(1)));
        var midiFile = new MidiFile(chunks);

        midiFile.ReplaceTempoMap(tempoMap);

        return midiFile;
    }

    private Dictionary<Voice, PatternBuilder> InitializePatternBuildersByVoice()
    {
        return compositionConfiguration.VoiceConfigurations.ToDictionary(
            voiceConfiguration => voiceConfiguration.Voice,
            voiceConfiguration => new PatternBuilder().ProgramChange(voiceConfiguration.Instrument)
        );
    }

    private void ProcessMeasure(Measure measure, Dictionary<Voice, PatternBuilder> patternBuildersByVoice)
    {
        foreach (var beat in measure.Beats)
        {
            foreach (var voice in compositionConfiguration.Voices)
            {
                var patternBuilder = patternBuildersByVoice[voice];

                if (!beat.ContainsVoice(voice))
                {
                    patternBuilder.StepForward(MusicalTimeSpan.Quarter);

                    continue;
                }

                var note = beat[voice];

                if (HandledRestfulOrnamentation(patternBuilder, note))
                {
                    continue;
                }

                patternBuilder.SetNoteLength(note.Duration).Note(note.Raw);

                foreach (var ornamentation in note.Ornamentations)
                {
                    patternBuilder.SetNoteLength(ornamentation.Duration).Note(ornamentation.Raw);
                }
            }
        }
    }

    private static bool HandledRestfulOrnamentation(PatternBuilder patternBuilder, BaroquenNote note) => note.OrnamentationType switch
    {
        OrnamentationType.MidSustain => true,
        OrnamentationType.Rest => HandleRest(patternBuilder),
        _ => false
    };

    private static bool HandleRest(PatternBuilder patternBuilder)
    {
        patternBuilder.StepForward(MusicalTimeSpan.Quarter);

        return true;
    }
}

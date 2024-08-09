using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Midi.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Midi;

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
        var chunks = patternBuildersByVoice.Values.Select((patternBuilder, channelNumber) => patternBuilder.Build().ToTrackChunk(tempoMap, FourBitNumber.Values[channelNumber]));
        var midiFile = new MidiFile(chunks);

        midiFile.ReplaceTempoMap(tempoMap);

        return midiFile;
    }

    private Dictionary<Voice, PatternBuilder> InitializePatternBuildersByVoice() => compositionConfiguration.VoiceConfigurations.ToDictionary(
        voiceConfiguration => voiceConfiguration.Voice,
        voiceConfiguration => new PatternBuilder().ProgramChange(voiceConfiguration.Instrument)
    );

    private void ProcessMeasure(Measure measure, Dictionary<Voice, PatternBuilder> patternBuildersByVoice)
    {
        foreach (var beat in measure.Beats)
        {
            ProcessBeat(beat, patternBuildersByVoice);
        }
    }

    private void ProcessBeat(Beat beat, Dictionary<Voice, PatternBuilder> patternBuildersByVoice)
    {
        foreach (var (voice, patternBuilder) in patternBuildersByVoice)
        {
            ProcessVoice(voice, beat, patternBuilder);
        }
    }

    private void ProcessVoice(Voice voice, Beat beat, PatternBuilder patternBuilder)
    {
        if (!beat.ContainsVoice(voice))
        {
            patternBuilder.AddRest(compositionConfiguration.DefaultNoteTimeSpan);

            return;
        }

        var note = beat[voice];

        if (HandledRestfulOrnamentation(patternBuilder, note))
        {
            return;
        }

        patternBuilder.AddNote(note);
    }

    /// <summary>
    ///     Determines if a "restful" ornamentation is needed and handles it.
    /// </summary>
    /// <param name="patternBuilder">The pattern builder to potentially handle a restful ornamentation in.</param>
    /// <param name="note">The note, which might have a restful ornamentation.</param>
    /// <returns>Whether a restful ornamentation was handled.</returns>
    private bool HandledRestfulOrnamentation(PatternBuilder patternBuilder, BaroquenNote note)
    {
        switch (note.OrnamentationType)
        {
            case OrnamentationType.MidSustain:
                return true;
            case OrnamentationType.Rest:
                patternBuilder.AddRest(compositionConfiguration.DefaultNoteTimeSpan);
                return true;
            default:
                return false;
        }
    }
}

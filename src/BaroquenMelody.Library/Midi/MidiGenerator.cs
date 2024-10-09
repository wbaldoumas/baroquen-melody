using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.Midi.Extensions;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Midi;

/// <inheritdoc cref="IMidiGenerator"/>
internal sealed class MidiGenerator(CompositionConfiguration compositionConfiguration) : IMidiGenerator
{
    public MidiFile Generate(Composition composition)
    {
        var patternBuildersByInstrument = InitializePatternBuildersByInstrument();

        foreach (var measure in composition.Measures)
        {
            ProcessMeasure(measure, patternBuildersByInstrument);
        }

        var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(compositionConfiguration.Tempo), compositionConfiguration.Meter.ToTimeSignature());
        var chunks = patternBuildersByInstrument.Values.Select((patternBuilder, channelNumber) => patternBuilder.Build().ToTrackChunk(tempoMap, FourBitNumber.Values[channelNumber]));
        var midiFile = new MidiFile(chunks);

        midiFile.ReplaceTempoMap(tempoMap);

        return midiFile;
    }

    private FrozenDictionary<Instrument, PatternBuilder> InitializePatternBuildersByInstrument() => compositionConfiguration.InstrumentConfigurations.ToDictionary(
        instrumentConfiguration => instrumentConfiguration.Instrument,
        instrumentConfiguration => new PatternBuilder().ProgramChange(instrumentConfiguration.MidiProgram)
    ).ToFrozenDictionary();

    private void ProcessMeasure(Measure measure, FrozenDictionary<Instrument, PatternBuilder> patternBuildersByInstrument)
    {
        foreach (var beat in measure.Beats)
        {
            ProcessBeat(beat, patternBuildersByInstrument);
        }
    }

    private void ProcessBeat(Beat beat, FrozenDictionary<Instrument, PatternBuilder> patternBuildersByInstrument)
    {
        foreach (var (instrument, patternBuilder) in patternBuildersByInstrument)
        {
            ProcessInstrument(instrument, beat, patternBuilder);
        }
    }

    private void ProcessInstrument(Instrument instrument, Beat beat, PatternBuilder patternBuilder)
    {
        if (!beat.ContainsInstrument(instrument))
        {
            patternBuilder.AddRest(compositionConfiguration.DefaultNoteTimeSpan);

            return;
        }

        var note = beat[instrument];

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

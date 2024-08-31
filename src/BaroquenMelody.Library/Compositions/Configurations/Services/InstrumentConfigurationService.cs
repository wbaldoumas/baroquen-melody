﻿using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using Melanchall.DryWetMidi.Standards;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class InstrumentConfigurationService(
    IDispatcher dispatcher,
    IState<CompositionConfigurationState> compositionConfigurationState
) : IInstrumentConfigurationService
{
    private static readonly FrozenSet<Instrument> _configurableInstruments = new[]
    {
        Instrument.One,
        Instrument.Two,
        Instrument.Three,
        Instrument.Four
    }.ToFrozenSet();

    public IEnumerable<Instrument> ConfigurableInstruments => _configurableInstruments;

    public void ConfigureDefaults()
    {
        ConfigureDefaults(Instrument.One);
        ConfigureDefaults(Instrument.Two);
        ConfigureDefaults(Instrument.Three);
        ConfigureDefaults(Instrument.Four, isEnabled: false);
    }

    public void Randomize()
    {
        Randomize(Instrument.One);
        Randomize(Instrument.Two);
        Randomize(Instrument.Three);
        Randomize(Instrument.Four);
    }

    private void ConfigureDefaults(Instrument instrument, bool isEnabled = true)
    {
        var defaultConfiguration = InstrumentConfiguration.DefaultConfigurations[instrument];

        var closestMinNote = compositionConfigurationState.Value.Scale.GetNotes()
            .OrderBy(note => Math.Abs(note.NoteNumber - defaultConfiguration.MinNote.NoteNumber))
            .First();

        var closestMaxNote = compositionConfigurationState.Value.Scale.GetNotes()
            .OrderBy(note => Math.Abs(note.NoteNumber - defaultConfiguration.MaxNote.NoteNumber))
            .First();

        dispatcher.Dispatch(
            new UpdateInstrumentConfiguration(
                instrument,
                closestMinNote,
                closestMaxNote,
                defaultConfiguration.MidiProgram,
                isEnabled,
                IsUserApplied: true
            )
        );
    }

    private void Randomize(Instrument instrument)
    {
        var minNoteIndex = ThreadLocalRandom.Next(0, compositionConfigurationState.Value.Notes.Count - CompositionConfiguration.MinInstrumentRange);
        var maxNoteIndex = ThreadLocalRandom.Next(minNoteIndex + CompositionConfiguration.MinInstrumentRange, Math.Min(compositionConfigurationState.Value.Notes.Count, minNoteIndex + CompositionConfiguration.MaxInstrumentRange));

        var minNote = compositionConfigurationState.Value.Notes[minNoteIndex];
        var maxNote = compositionConfigurationState.Value.Notes[maxNoteIndex];

        var midiInstruments = EnumUtils<GeneralMidi2Program>.AsEnumerable().ToList();
        var midiInstrument = midiInstruments[ThreadLocalRandom.Next(0, midiInstruments.Count)];

        var isEnabled = ThreadLocalRandom.Next(0, 2) == 0;

        dispatcher.Dispatch(
            new UpdateInstrumentConfiguration(
                instrument,
                minNote,
                maxNote,
                midiInstrument,
                isEnabled,
                IsUserApplied: true
            )
        );
    }
}

using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Midi.Repositories;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations.Services;

internal sealed class InstrumentConfigurationService(
    IMidiInstrumentRepository midiInstrumentRepository,
    IDispatcher dispatcher,
    IState<CompositionConfigurationState> compositionConfigurationState,
    IState<InstrumentConfigurationState> instrumentConfigurationState
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
        ConfigureDefaults(Instrument.Four, status: ConfigurationStatus.Disabled);
    }

    public void Randomize()
    {
        Randomize(Instrument.One);
        Randomize(Instrument.Two);
        Randomize(Instrument.Three);
        Randomize(Instrument.Four);
    }

    private void ConfigureDefaults(Instrument instrument, ConfigurationStatus status = ConfigurationStatus.Enabled)
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
                status,
                IsUserApplied: true
            )
        );
    }

    private void Randomize(Instrument instrument)
    {
        if (instrumentConfigurationState.Value.Configurations[instrument].IsFrozen)
        {
            return;
        }

        var status = instrumentConfigurationState.Value.Configurations[instrument].Status;

        var minNoteIndex = ThreadLocalRandom.Next(0, compositionConfigurationState.Value.Notes.Count - CompositionConfiguration.MinInstrumentRange);
        var maxNoteIndex = ThreadLocalRandom.Next(minNoteIndex + CompositionConfiguration.MinInstrumentRange, Math.Min(compositionConfigurationState.Value.Notes.Count, minNoteIndex + CompositionConfiguration.MaxInstrumentRange));

        var minNote = compositionConfigurationState.Value.Notes[minNoteIndex];
        var maxNote = compositionConfigurationState.Value.Notes[maxNoteIndex];

        var midiInstruments = midiInstrumentRepository.GetAllMidiInstruments().ToList();
        var midiInstrument = midiInstruments[ThreadLocalRandom.Next(0, midiInstruments.Count)];

        dispatcher.Dispatch(
            new UpdateInstrumentConfiguration(
                instrument,
                minNote,
                maxNote,
                midiInstrument,
                status,
                IsUserApplied: true
            )
        );
    }
}

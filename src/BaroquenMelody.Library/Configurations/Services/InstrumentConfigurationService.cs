using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Midi.Repositories;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations.Services;

internal sealed class InstrumentConfigurationService(
    IMidiInstrumentRepository midiInstrumentRepository,
    IDispatcher dispatcher,
    IState<CompositionConfigurationState> compositionConfigurationState,
    IState<InstrumentConfigurationState> instrumentConfigurationState
) : IInstrumentConfigurationService
{
    private const int MinMinRandomVelocity = 25;

    private const int MaxMinRandomVelocity = 67;

    private const int MaxMaxRandomVelocity = 101;

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
                defaultConfiguration.MinVelocity,
                defaultConfiguration.MaxVelocity,
                defaultConfiguration.MidiProgram,
                status,
                IsUserApplied: true
            )
        );
    }

    private void Randomize(Instrument instrument)
    {
        var instrumentConfiguration = instrumentConfigurationState.Value.Configurations[instrument];

        if (instrumentConfiguration.IsFrozen)
        {
            return;
        }

        var minVelocity = ThreadLocalRandom.Next(MinMinRandomVelocity, MaxMinRandomVelocity);
        var maxVelocity = ThreadLocalRandom.Next(minVelocity, MaxMaxRandomVelocity);

        var minRandomIndex = compositionConfigurationState.Value.AvailableNotes.IndexOf(Notes.C1);
        var maxRandomIndex = compositionConfigurationState.Value.AvailableNotes.IndexOf(Notes.C7);

        var minNoteIndex = ThreadLocalRandom.Next(minRandomIndex, maxRandomIndex - CompositionConfiguration.MinInstrumentRange);
        var maxNoteIndex = ThreadLocalRandom.Next(minNoteIndex + CompositionConfiguration.MinInstrumentRange, Math.Min(maxRandomIndex, minNoteIndex + CompositionConfiguration.MaxInstrumentRange));

        var minNote = compositionConfigurationState.Value.AvailableNotes[minNoteIndex];
        var maxNote = compositionConfigurationState.Value.AvailableNotes[maxNoteIndex];

        var midiInstruments = midiInstrumentRepository.GetAllMidiInstruments().ToList();
        var midiInstrument = midiInstruments[ThreadLocalRandom.Next(0, midiInstruments.Count)];

        dispatcher.Dispatch(
            new UpdateInstrumentConfiguration(
                instrument,
                minNote,
                maxNote,
                new SevenBitNumber((byte)minVelocity),
                new SevenBitNumber((byte)maxVelocity),
                midiInstrument,
                instrumentConfiguration.Status,
                IsUserApplied: true
            )
        );
    }
}

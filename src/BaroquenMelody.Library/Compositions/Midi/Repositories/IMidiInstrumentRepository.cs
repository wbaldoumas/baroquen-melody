using BaroquenMelody.Library.Compositions.Midi.Enums;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Compositions.Midi.Repositories;

public interface IMidiInstrumentRepository
{
    /// <summary>
    ///     Get the <see cref="GeneralMidi2Program"/>s for the given <see cref="MidiInstrumentType"/>.
    /// </summary>
    /// <param name="midiInstrumentType">The <see cref="MidiInstrumentType"/> to get the <see cref="GeneralMidi2Program"/>s for.</param>
    /// <returns>The <see cref="GeneralMidi2Program"/>s for the given <see cref="MidiInstrumentType"/>.</returns>
    IEnumerable<GeneralMidi2Program> GetMidiInstruments(MidiInstrumentType midiInstrumentType);

    /// <summary>
    ///     Get all <see cref="GeneralMidi2Program"/>s.
    /// </summary>
    /// <returns>All <see cref="GeneralMidi2Program"/>s.</returns>
    IEnumerable<GeneralMidi2Program> GetAllMidiInstruments();
}

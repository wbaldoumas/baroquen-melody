using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Midi;

public interface IMidiExampleGenerator
{
    /// <summary>
    ///     Generate a <see cref="MidiFile"/> example for the given <see cref="GeneralMidi2Program"/> and <see cref="Note"/>.
    /// </summary>
    /// <param name="midiProgram">The <see cref="GeneralMidi2Program"/> to generate a <see cref="MidiFile"/> example for.</param>
    /// <param name="note">The <see cref="Note"/> to generate a <see cref="MidiFile"/> example for.</param>
    /// <returns>A <see cref="MidiFile"/> example for the given <see cref="GeneralMidi2Program"/> and <see cref="Note"/>.</returns>
    MidiFile GenerateExampleNoteMidiFile(GeneralMidi2Program midiProgram, Note note);
}

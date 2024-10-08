using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
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

    /// <summary>
    ///     Generates a <see cref="MidiFile"/> example for the given <see cref="OrnamentationType"/>.
    /// </summary>
    /// <param name="ornamentationType">The <see cref="OrnamentationType"/> to generate a <see cref="MidiFile"/> example for.</param>
    /// <param name="compositionConfiguration">The current <see cref="CompositionConfiguration"/> to generate a <see cref="MidiFile"/> example for.</param>
    /// <returns>The <see cref="MidiFile"/> example for the given <see cref="OrnamentationType"/>.</returns>
    MidiFile GenerateExampleOrnamentationMidiFile(OrnamentationType ornamentationType, CompositionConfiguration compositionConfiguration);
}

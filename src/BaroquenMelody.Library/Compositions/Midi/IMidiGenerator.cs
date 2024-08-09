using BaroquenMelody.Library.Compositions.Domain;
using Melanchall.DryWetMidi.Core;

namespace BaroquenMelody.Library.Compositions.Midi;

/// <summary>
///     Generates a <see cref="MidiFile"/> from a <see cref="Composition"/>.
/// </summary>
internal interface IMidiGenerator
{
    /// <summary>
    ///     Generate a <see cref="MidiFile"/> from the given <see cref="Composition"/>.
    /// </summary>
    /// <param name="composition">The <see cref="Composition"/> to generate a <see cref="MidiFile"/> from.</param>
    /// <returns>A <see cref="MidiFile"/> representing the given <see cref="Composition"/>.</returns>
    MidiFile Generate(Composition composition);
}

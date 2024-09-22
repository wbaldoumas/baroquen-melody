namespace BaroquenMelody.Library.Compositions.Midi.Enums;

[Flags]
public enum MidiInstrumentType
{
    /// <summary>
    ///     No instrument type.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Keyboards.
    /// </summary>
    Keyboard = 1 << 0,

    /// <summary>
    ///     Chromatic percussion instruments.
    /// </summary>
    ChromaticPercussion = 1 << 1,

    /// <summary>
    ///     Organs.
    /// </summary>
    Organ = 1 << 2,

    /// <summary>
    ///     Guitars.
    /// </summary>
    Guitar = 1 << 3,

    /// <summary>
    ///     Basses.
    /// </summary>
    Bass = 1 << 4,

    /// <summary>
    ///     Strings.
    /// </summary>
    Strings = 1 << 5,

    /// <summary>
    ///     Ensemble instruments.
    /// </summary>
    Ensemble = 1 << 6,

    /// <summary>
    ///     Voices.
    /// </summary>
    Voice = 1 << 7,

    /// <summary>
    ///     Brass instruments.
    /// </summary>
    Brass = 1 << 8,

    /// <summary>
    ///     Woodwind instruments.
    /// </summary>
    Woodwind = 1 << 9,

    /// <summary>
    ///     Synthesizers.
    /// </summary>
    Synth = 1 << 10,

    /// <summary>
    ///     All instrument types.
    /// </summary>
    All = Keyboard | ChromaticPercussion | Organ | Guitar | Bass | Strings | Ensemble | Voice | Brass | Woodwind | Synth
}

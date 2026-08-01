using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Enums.Extensions;
using BaroquenMelody.Library.Configurations.Serialization.JsonConverters;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///    The instrument configuration. Allowing for the configuration of the note range, dynamics, and MIDI instrument
///     for a given instrument. Also allows for disabling the instrument entirely.
/// </summary>
/// <param name="Instrument"> The instrument to be configured. </param>
/// <param name="MinNote"> The instrument's minimum note value. </param>
/// <param name="MaxNote"> The instrument's maximum note value. </param>
/// <param name="MinVelocity"> The instrument's minimum velocity value, impacting dynamics. </param>
/// <param name="MaxVelocity"> The instrument's maximum velocity value, impacting dynamics. </param>
/// <param name="Status"> Whether the instrument is enabled, locked, or disabled. </param>
/// <param name="MidiProgram"> The instrument's midi program. </param>
public sealed record InstrumentConfiguration(
    Instrument Instrument,
    [property: JsonConverter(typeof(NoteJsonConverter))]
    Note MinNote,
    [property: JsonConverter(typeof(NoteJsonConverter))]
    Note MaxNote,
    [property: JsonConverter(typeof(SevenBitNumberJsonConverter))]
    SevenBitNumber MinVelocity,
    [property: JsonConverter(typeof(SevenBitNumberJsonConverter))]
    SevenBitNumber MaxVelocity,
    GeneralMidi2Program MidiProgram,
    ConfigurationStatus Status)
{
    private const int MinValidVelocityRange = 5;

    public static readonly SevenBitNumber DefaultMinVelocity = new(50);

    public static readonly SevenBitNumber DefaultMaxVelocity = new(60);

    [JsonIgnore]
    public bool IsEnabled => Status.IsEnabled();

    [JsonIgnore]
    public bool IsFrozen => Status.IsFrozen();

    [JsonIgnore]
    public int VelocityRange => MaxVelocity - MinVelocity;

    [JsonIgnore]
    public bool HasSizeableVelocityRange => VelocityRange > MinValidVelocityRange;

    public bool IsNoteWithinInstrumentRange(Note note) => MinNote.NoteNumber <= note.NoteNumber && note.NoteNumber <= MaxNote.NoteNumber;

    /// <summary>
    ///     The default ranges keep adjacent voices' range centers within the voice spacing rule's twelve-semitone
    ///     cap (the lowest pair, exempt from the rule, sits slightly wider), so the rule never fights the ranges'
    ///     natural tessitura: every playable note of every voice can appear in some rule-satisfying voicing.
    ///     The lower three voices' floors each sit a full octave below an in-range C, so whichever voice is
    ///     lowest can carry every ground bass pattern in the default key - the octave-descending grounds need
    ///     the tonic with a whole scale octave beneath it - and their tessituras land on the classical alto
    ///     (Two), tenor (Three), and bass (Four) ranges.
    /// </summary>
    public static readonly FrozenDictionary<Instrument, InstrumentConfiguration> DefaultConfigurations = new Dictionary<Instrument, InstrumentConfiguration>
    {
        { Instrument.One, new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, DefaultMinVelocity, DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled) },
        { Instrument.Two, new InstrumentConfiguration(Instrument.Two, Notes.C4, Notes.G5, DefaultMinVelocity, DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled) },
        { Instrument.Three, new InstrumentConfiguration(Instrument.Three, Notes.C3, Notes.B4, DefaultMinVelocity, DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled) },
        { Instrument.Four, new InstrumentConfiguration(Instrument.Four, Notes.C2, Notes.A3, DefaultMinVelocity, DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Disabled) }
    }.ToFrozenDictionary();
}

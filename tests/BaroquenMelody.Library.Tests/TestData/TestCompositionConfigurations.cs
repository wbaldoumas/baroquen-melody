using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Tests.TestData;

internal static class TestCompositionConfigurations
{
    public static CompositionConfiguration Get(
        int numberOfInstruments = 4,
        int compositionLength = 100,
        NoteName tonic = NoteName.C,
        Mode mode = Mode.Ionian,
        SevenBitNumber? minVelocity = null,
        SevenBitNumber? maxVelocity = null
    ) => new(
        GenerateInstrumentConfigurations(numberOfInstruments, minVelocity ?? InstrumentConfiguration.DefaultMinVelocity, maxVelocity ?? InstrumentConfiguration.DefaultMaxVelocity),
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        tonic,
        mode,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: compositionLength
    );

    private static HashSet<InstrumentConfiguration> GenerateInstrumentConfigurations(int numberOfInstruments, SevenBitNumber minVelocity, SevenBitNumber maxVelocity) => numberOfInstruments switch
    {
        0 => [],
        1 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        ],
        2 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Two, Notes.G2, Notes.G4, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        ],
        3 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Two, Notes.G2, Notes.G4, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Three, Notes.C2, Notes.C3, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        ],
        4 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Two, Notes.G2, Notes.G4, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Three, Notes.C2, Notes.C3, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Four, Notes.C1, Notes.C2, minVelocity, maxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        ],
        _ => throw new ArgumentException("Invalid number of instruments.")
    };
}

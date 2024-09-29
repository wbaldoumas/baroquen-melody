﻿using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Tests.TestData;

internal static class TestCompositionConfigurations
{
    public static CompositionConfiguration Get(
        int numberOfInstruments = 4,
        int compositionLength = 100
    ) => new(
        GenerateInstrumentConfigurations(numberOfInstruments),
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: compositionLength
    );

    private static HashSet<InstrumentConfiguration> GenerateInstrumentConfigurations(int numberOfInstruments) => numberOfInstruments switch
    {
        0 => [],
        1 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6)
        ],
        2 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6),
            new InstrumentConfiguration(Instrument.Two, Notes.G2, Notes.G4)
        ],
        3 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6),
            new InstrumentConfiguration(Instrument.Two, Notes.G2, Notes.G4),
            new InstrumentConfiguration(Instrument.Three, Notes.C2, Notes.C3)
        ],
        4 =>
        [
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6),
            new InstrumentConfiguration(Instrument.Two, Notes.G2, Notes.G4),
            new InstrumentConfiguration(Instrument.Three, Notes.C2, Notes.C3),
            new InstrumentConfiguration(Instrument.Four, Notes.C1, Notes.C2)
        ],
        _ => throw new ArgumentException("Invalid number of instruments.")
    };
}

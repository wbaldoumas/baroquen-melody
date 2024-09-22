using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Midi.Enums;
using BaroquenMelody.Library.Compositions.Midi.Repositories;
using BaroquenMelody.Library.Infrastructure.Random;
using FluentAssertions;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Midi.Repositories;

[TestFixture]
internal sealed class MidiInstrumentRepositoryTests
{
    private MidiInstrumentRepository _midiInstrumentRepository = null!;

    [SetUp]
    public void SetUp() => _midiInstrumentRepository = new MidiInstrumentRepository();

    [Test]
    [TestCase(MidiInstrumentType.Keyboard, MidiInstrumentType.ChromaticPercussion, GeneralMidi2Program.AcousticGrandPiano, GeneralMidi2Program.Vibraphone, GeneralMidi2Program.AcousticGuitarNylon)]
    [TestCase(MidiInstrumentType.Organ, MidiInstrumentType.Guitar, GeneralMidi2Program.ChurchOrgan, GeneralMidi2Program.AcousticGuitarNylon, GeneralMidi2Program.AcousticGrandPiano)]
    [TestCase(MidiInstrumentType.Bass, MidiInstrumentType.Strings, GeneralMidi2Program.AcousticBass, GeneralMidi2Program.Violin, GeneralMidi2Program.AcousticGrandPiano)]
    [TestCase(MidiInstrumentType.Ensemble, MidiInstrumentType.Voice, GeneralMidi2Program.StringEnsembles1, GeneralMidi2Program.ChoirAahs, GeneralMidi2Program.AcousticGuitarNylon)]
    [TestCase(MidiInstrumentType.Brass, MidiInstrumentType.Woodwind, GeneralMidi2Program.Trumpet, GeneralMidi2Program.Flute, GeneralMidi2Program.AcousticGrandPiano)]
    [TestCase(MidiInstrumentType.Keyboard, MidiInstrumentType.Organ, GeneralMidi2Program.AcousticGrandPiano, GeneralMidi2Program.ChurchOrgan, GeneralMidi2Program.AcousticGuitarNylon)]
    [TestCase(MidiInstrumentType.ChromaticPercussion, MidiInstrumentType.Guitar, GeneralMidi2Program.Vibraphone, GeneralMidi2Program.AcousticGuitarNylon, GeneralMidi2Program.AcousticGrandPiano)]
    public void GetMidiInstruments_returns_expected_values(
        MidiInstrumentType midiInstrumentTypeA,
        MidiInstrumentType midiInstrumentTypeB,
        GeneralMidi2Program expectedInstrumentA,
        GeneralMidi2Program expectedInstrumentB,
        GeneralMidi2Program unexpectedInstrument)
    {
        // arrange
        var instrumentBitFlag = midiInstrumentTypeA | midiInstrumentTypeB;

        // act
        var midiInstruments = _midiInstrumentRepository.GetMidiInstruments(instrumentBitFlag).ToList();

        // assert
        midiInstruments.Should().Contain(expectedInstrumentA);
        midiInstruments.Should().Contain(expectedInstrumentB);
        midiInstruments.Should().NotContain(unexpectedInstrument);
    }
}

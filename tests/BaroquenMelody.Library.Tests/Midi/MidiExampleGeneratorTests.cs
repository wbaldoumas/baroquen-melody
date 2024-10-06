using BaroquenMelody.Library.Midi;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Midi;

[TestFixture]
internal sealed class MidiExampleGeneratorTests
{
    private MidiExampleGenerator _midiExampleGenerator = null!;

    [SetUp]
    public void SetUp() => _midiExampleGenerator = new MidiExampleGenerator();

    [Test]
    public void GenerateExampleMidiFile_WhenGivenValidInput_ReturnsMidiFile()
    {
        // arrange
        const GeneralMidi2Program midiProgram = GeneralMidi2Program.AcousticGrandPiano;

        // act
        var midiFile = _midiExampleGenerator.GenerateExampleNoteMidiFile(midiProgram, Notes.C4);

        // assert
        var notes = midiFile.GetNotes();

        notes.Should().HaveCount(1);

        var note = notes.First();

        note.Should().NotBeNull();
        note.NoteNumber.Should().Be(Notes.C4.NoteNumber);
    }
}

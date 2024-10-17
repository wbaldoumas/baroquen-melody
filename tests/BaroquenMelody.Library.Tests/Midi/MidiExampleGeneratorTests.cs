using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Providers;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Tests.Midi;

[TestFixture]
internal sealed class MidiExampleGeneratorTests
{
    private MidiExampleGenerator _midiExampleGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        _midiExampleGenerator = new MidiExampleGenerator(
            new MusicalTimeSpanCalculator(),
            new OrnamentationProcessorConfigurationFactoryProvider(
                new WeightedRandomBooleanGenerator(),
                Substitute.For<ILogger<MidiFileComposition>>()
            )
        );
    }

    [Test]
    public void GenerateExampleMidiFile_WhenGivenValidInput_ReturnsMidiFile()
    {
        // arrange
        const GeneralMidi2Program midiProgram = GeneralMidi2Program.AcousticGrandPiano;

        // act
        var midiFile = _midiExampleGenerator.GenerateExampleNoteMidiFile(midiProgram, Notes.C4);

        // assert
        var notes = midiFile.GetNotes();

        notes.Should().ContainSingle();

        var note = notes.First();

        note.Should().NotBeNull();
        note.NoteNumber.Should().Be(Notes.C4.NoteNumber);
    }

    [Test]
    public void GenerateExampleOrnamentationMidiFile_handles_all_ornamentation_types()
    {
        // arrange
        var excludedOrnamentationTypes = new HashSet<OrnamentationType>
        {
            OrnamentationType.None,
            OrnamentationType.Sustain,
            OrnamentationType.MidSustain,
            OrnamentationType.Rest
        }.ToFrozenSet();

        var ornamentationTypes = EnumUtils<OrnamentationType>.AsEnumerable()
            .Where(ornamentationType => !excludedOrnamentationTypes.Contains(ornamentationType))
            .ToList();

        // act
        foreach (var ornamentationType in ornamentationTypes)
        {
            var act = () => _midiExampleGenerator.GenerateExampleOrnamentationMidiFile(ornamentationType, TestCompositionConfigurations.Get());

            // assert
            act.Should().NotThrow();
        }
    }
}

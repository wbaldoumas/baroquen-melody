using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class NoteTransposerTests
{
    private NoteTransposer _noteTransposer = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _noteTransposer = new NoteTransposer(compositionConfiguration);
    }

    [Test]
    public void Transpose_TransposesNotesAsExpected()
    {
        // arrange
        var notesToTranspose = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Instrument.One, Notes.E4, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            },
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            }
        };

        var expectedNotes = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.G2, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Instrument.Two, Notes.A2, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Instrument.Two, Notes.B2, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Instrument.Two, Notes.D3, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            },
            new(Instrument.Two, Notes.G2, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            }
        };

        // act
        var transposedNotes = _noteTransposer.TransposeToInstrument(notesToTranspose, Instrument.One, Instrument.Two).ToList();

        // assert
        transposedNotes.Should().HaveCount(expectedNotes.Count);

        for (var i = 0; i < transposedNotes.Count; i++)
        {
            transposedNotes[i].Should().BeEquivalentTo(expectedNotes[i]);
        }
    }
}

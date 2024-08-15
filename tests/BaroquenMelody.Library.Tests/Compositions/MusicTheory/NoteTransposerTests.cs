using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.MusicTheory;

[TestFixture]
internal sealed class NoteTransposerTests
{
    private NoteTransposer _noteTransposer = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _noteTransposer = new NoteTransposer(compositionConfiguration);
    }

    [Test]
    public void Transpose_TransposesNotesAsExpected()
    {
        // arrange
        var notesToTranspose = new List<BaroquenNote>
        {
            new(Voice.One, Notes.C4, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Voice.One, Notes.D4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Voice.One, Notes.E4, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Voice.One, Notes.F4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Voice.One, Notes.G4, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            },
            new(Voice.One, Notes.C4, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            }
        };

        var expectedNotes = new List<BaroquenNote>
        {
            new(Voice.Two, Notes.G2, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Voice.Two, Notes.A2, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Voice.Two, Notes.B2, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            },
            new(Voice.Two, Notes.D3, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            },
            new(Voice.Two, Notes.G2, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.None
            }
        };

        // act
        var transposedNotes = _noteTransposer.TransposeToVoice(notesToTranspose, Voice.One, Voice.Two).ToList();

        // assert
        transposedNotes.Should().HaveCount(expectedNotes.Count);

        for (var i = 0; i < transposedNotes.Count; i++)
        {
            transposedNotes[i].Should().BeEquivalentTo(expectedNotes[i]);
        }
    }
}

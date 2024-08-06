using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
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
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C4, Notes.C6),
                new(Voice.Alto, Notes.G2, Notes.G4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _noteTransposer = new NoteTransposer(compositionConfiguration);
    }

    [Test]
    public void Transpose_TransposesNotesAsExpected()
    {
        // arrange
        var notesToTranspose = new List<BaroquenNote>
        {
            new(Voice.Soprano, Notes.C4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                MusicalTimeSpan = MusicalTimeSpan.Eighth,
                Ornamentations =
                {
                    new BaroquenNote(Voice.Soprano, Notes.D4)
                    {
                        OrnamentationType = OrnamentationType.PassingTone,
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                }
            },
            new(Voice.Soprano, Notes.E4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                MusicalTimeSpan = MusicalTimeSpan.Eighth,
                Ornamentations =
                {
                    new BaroquenNote(Voice.Soprano, Notes.F4)
                    {
                        OrnamentationType = OrnamentationType.PassingTone,
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                }
            },
            new(Voice.Soprano, Notes.G4)
            {
                OrnamentationType = OrnamentationType.None,
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            },
            new(Voice.Soprano, Notes.C4)
            {
                OrnamentationType = OrnamentationType.None,
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            }
        };

        var expectedNotes = new List<BaroquenNote>
        {
            new(Voice.Alto, Notes.G2)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                MusicalTimeSpan = MusicalTimeSpan.Eighth,
                Ornamentations =
                {
                    new BaroquenNote(Voice.Alto, Notes.A2)
                    {
                        OrnamentationType = OrnamentationType.PassingTone,
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                }
            },
            new(Voice.Alto, Notes.B2)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                MusicalTimeSpan = MusicalTimeSpan.Eighth,
                Ornamentations =
                {
                    new BaroquenNote(Voice.Alto, Notes.C3)
                    {
                        OrnamentationType = OrnamentationType.PassingTone,
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                }
            },
            new(Voice.Alto, Notes.D3)
            {
                OrnamentationType = OrnamentationType.None,
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            },
            new(Voice.Alto, Notes.G2)
            {
                OrnamentationType = OrnamentationType.None,
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            }
        };

        // act
        var transposedNotes = _noteTransposer.TransposeToVoice(notesToTranspose, Voice.Soprano, Voice.Alto).ToList();

        // assert
        transposedNotes.Should().HaveCount(expectedNotes.Count);

        for (var i = 0; i < transposedNotes.Count; i++)
        {
            transposedNotes[i].Should().BeEquivalentTo(expectedNotes[i]);
        }
    }
}

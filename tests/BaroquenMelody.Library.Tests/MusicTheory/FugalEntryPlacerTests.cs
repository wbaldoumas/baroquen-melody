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
internal sealed class FugalEntryPlacerTests
{
    private FugalEntryPlacer _fugalEntryPlacer = null!;

    [SetUp]
    public void SetUp()
    {
        // C major (Ionian); ranges: One C4-C6, Two G2-G4, Three C2-C3, Four C1-C2.
        var compositionConfiguration = TestCompositionConfigurations.Get(4);

        _fugalEntryPlacer = new FugalEntryPlacer(compositionConfiguration);
    }

    [Test]
    public void Place_WhenOneOctaveFullyFits_TransposesEntryToThatOctave()
    {
        // arrange: into Two (G2-G4); only dropping an octave seats both C4 and E5 fully in range.
        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E5, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.C3, MusicalTimeSpan.Half),
            new(Instrument.Two, Notes.E4, MusicalTimeSpan.Half)
        };

        // act
        var placed = _fugalEntryPlacer.Place(entry, Instrument.Two);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_WhenSeveralOctavesFullyFit_PrefersTheMostCenteredOctave()
    {
        // arrange: into Two (G2-G4, center G3); G2, G3 and G4 all fit, G3 is the most centered.
        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        };

        // act
        var placed = _fugalEntryPlacer.Place(entry, Instrument.Two);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_WhenTwoOctavesAreEquallyCentered_PrefersTheLowerOctave()
    {
        // arrange: into Three (C2-C3, center F#1); C2 and C3 both fit and are equidistant from center, C2 is lower.
        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Three, Notes.C2, MusicalTimeSpan.Half)
        };

        // act
        var placed = _fugalEntryPlacer.Place(entry, Instrument.Three);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_WhenEntryIsTallerThanRange_MinimizesSpillAndLeavesSpilledNotesAtTruePitch()
    {
        // arrange: into Three (C2-C3); the entry spans more than an octave so no placement fully fits.
        // Dropping two octaves leaves only E3 above the range (one spilled note), the fewest possible,
        // and E3 is kept at its true pitch rather than clamped into range.
        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E5, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Three, Notes.C2, MusicalTimeSpan.Half),
            new(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
            new(Instrument.Three, Notes.E3, MusicalTimeSpan.Half)
        };

        // act
        var placed = _fugalEntryPlacer.Place(entry, Instrument.Three);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_ShiftsOrnamentationWithItsPrincipalNote()
    {
        // arrange: into Two (G2-G4); the ornament E6 is the highest note, so only dropping two octaves fully fits,
        // and the ornament must move by the same two octaves as its principal.
        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C5, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Instrument.One, Notes.E6, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            }
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(Instrument.Two, Notes.E4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.PassingTone
                    }
                }
            }
        };

        // act
        var placed = _fugalEntryPlacer.Place(entry, Instrument.Two);

        // assert
        AssertEquivalent(placed, expected);
    }

    private static void AssertEquivalent(IReadOnlyList<BaroquenNote> actual, List<BaroquenNote> expected)
    {
        actual.Should().HaveCount(expected.Count);

        for (var i = 0; i < actual.Count; i++)
        {
            actual[i].Should().BeEquivalentTo(expected[i]);
        }
    }
}

using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using CsCheck;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class FugalEntryPlacerTests
{
    private CompositionConfiguration _compositionConfiguration = null!;

    private FugalEntryPlacer _fugalEntryPlacer = null!;

    [SetUp]
    public void SetUp()
    {
        // C major (Ionian); ranges: One C4-C6, Two G2-G4, Three C2-C3, Four C1-C2.
        _compositionConfiguration = TestCompositionConfigurations.Get(4);

        _fugalEntryPlacer = new FugalEntryPlacer(_compositionConfiguration);
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

    [Test]
    public void Place_WhenEntryIsBelowRange_TransposesUpToFit()
    {
        // arrange: into One (C4-C6); the entry sits below the range, so only shifting up an octave seats it fully.
        var entry = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.C3, MusicalTimeSpan.Half),
            new(Instrument.Two, Notes.E4, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new(Instrument.One, Notes.E5, MusicalTimeSpan.Half)
        };

        // act
        var placed = _fugalEntryPlacer.Place(entry, Instrument.One);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_WithSpacingFeasibleNotes_PrefersTheOctaveWhereEveryPrincipalNoteIsFeasible()
    {
        // arrange: into Two (G2-G4); without spacing sets a lone G4 would sit at the centered G3, but when the
        // voice spacing rule only admits G4 for this voice, the feasible octave must win over the centered one.
        var spacingFeasibleNoteNumbersByInstrument = new Dictionary<Instrument, FrozenSet<int>>
        {
            [Instrument.Two] = new[] { (int)Notes.G4.NoteNumber }.ToFrozenSet()
        }.ToFrozenDictionary();

        var fugalEntryPlacer = new FugalEntryPlacer(_compositionConfiguration, spacingFeasibleNoteNumbersByInstrument);

        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.G4, MusicalTimeSpan.Half)
        };

        // act
        var placed = fugalEntryPlacer.Place(entry, Instrument.Two);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_WithSpacingFeasibleNotesForOtherInstrumentsOnly_PlacesAsIfUnconstrained()
    {
        // arrange: the feasible sets constrain a different voice, so placement into Two behaves exactly as
        // without them and the centered G3 wins.
        var spacingFeasibleNoteNumbersByInstrument = new Dictionary<Instrument, FrozenSet<int>>
        {
            [Instrument.One] = new[] { (int)Notes.C5.NoteNumber }.ToFrozenSet()
        }.ToFrozenDictionary();

        var fugalEntryPlacer = new FugalEntryPlacer(_compositionConfiguration, spacingFeasibleNoteNumbersByInstrument);

        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.G4, MusicalTimeSpan.Half)
        };

        var expected = new List<BaroquenNote>
        {
            new(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        };

        // act
        var placed = fugalEntryPlacer.Place(entry, Instrument.Two);

        // assert
        AssertEquivalent(placed, expected);
    }

    [Test]
    public void Place_WithAPrecedingNote_AvoidsOctavesTheVoiceCannotStepTo()
    {
        // arrange: into Two (G2-G4); a lone C4 would sit at the more centered C4, but from a preceding G2 the
        // voice can only move five scale steps per beat, which reaches C3 (three steps) and not C4 (ten steps).
        var entry = new List<BaroquenNote>
        {
            new(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        };

        // act
        var placedWithoutPrecedingNote = _fugalEntryPlacer.Place(entry, Instrument.Two);
        var placedFromG2 = _fugalEntryPlacer.Place(entry, Instrument.Two, new BaroquenNote(Instrument.Two, Notes.G2, MusicalTimeSpan.Half));

        // assert
        AssertEquivalent(placedWithoutPrecedingNote, [new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)]);
        AssertEquivalent(placedFromG2, [new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)]);
    }

    [Test]
    public void Place_WhenEntryIsEmpty_ReturnsEmpty()
    {
        // act
        var placed = _fugalEntryPlacer.Place([], Instrument.One);

        // assert
        placed.Should().BeEmpty();
    }

    [Test]
    public void Place_PreservesEachNotesPitchClassCountAndInstrument()
    {
        var notes = _compositionConfiguration.Scale.GetNotes();
        var instruments = _compositionConfiguration.Instruments;

        // A comfortable middle band (C2-C7) so every entry can be octave-shifted in either direction within the scale.
        var lowestIndex = notes.FindIndex(note => (int)note.NoteNumber >= (int)Notes.C2.NoteNumber);
        var highestIndex = notes.FindLastIndex(note => (int)note.NoteNumber <= (int)Notes.C7.NoteNumber);

        var generateEntryIndices = Gen.Int[lowestIndex, highestIndex].List[1, 8];
        var generateTargetIndex = Gen.Int[0, instruments.Count - 1];

        Gen.Select(generateEntryIndices, generateTargetIndex, (indices, targetIndex) => (indices, targetIndex)).Sample(
            generated =>
            {
                var entry = generated.indices.Select(index => new BaroquenNote(Instrument.One, notes[index], MusicalTimeSpan.Half)).ToList();
                var target = instruments[generated.targetIndex];

                var placed = _fugalEntryPlacer.Place(entry, target);
                var placedAgain = _fugalEntryPlacer.Place(entry, target);

                placed.Should().HaveCount(entry.Count, "placement must never add or drop notes");

                for (var i = 0; i < placed.Count; i++)
                {
                    placed[i].Instrument.Should().Be(target, "every placed note belongs to the target voice");
                    placed[i].NoteName.Should().Be(entry[i].NoteName, "whole-octave placement preserves each note's pitch class");
                    placed[i].Should().BeEquivalentTo(placedAgain[i], "placement is deterministic");
                }
            },
            iter: 200
        );
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

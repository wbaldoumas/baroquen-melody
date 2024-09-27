using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Configuration;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors;

[TestFixture]
internal sealed class OrnamentationCleanerTests
{
    private INotePairSelector _mockNotePairSelector = null!;

    private INoteTargetSelector _mockNoteTargetSelector = null!;

    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    private OrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNotePairSelector = Substitute.For<INotePairSelector>();
        _mockNoteTargetSelector = Substitute.For<INoteTargetSelector>();
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
    }

    [Test]
    public void Process_cleans_notes_when_ornamentations_conflict()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
        {
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth)
            }
        };

        var secondaryNote = new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.DoubleRun,
            Ornamentations =
            {
                new BaroquenNote(Instrument.Two, Notes.F4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.Two, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.Two, Notes.F4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.Two, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Sixteenth)
            }
        };

        var indicesToCheck = new[]
        {
            new NoteIndexPair(0, 1, 0),
            new NoteIndexPair(1, 3, 1),
            new NoteIndexPair(2, 5, 2)
        };

        _mockNotePairSelector.Select(Arg.Any<OrnamentationCleaningItem>()).Returns(new NotePair(primaryNote, secondaryNote));
        _mockNoteTargetSelector.Select(Arg.Any<OrnamentationCleaningItem>()).Returns(primaryNote);

        var expectedPrimaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var expectedSecondaryNote = new BaroquenNote(secondaryNote);

        _cleaner = new OrnamentationCleaner(
            new OrnamentationCleanerConfiguration(_mockNotePairSelector, indicesToCheck, _mockNoteTargetSelector),
            Configurations.GetCompositionConfiguration(2),
            _mockWeightedRandomBooleanGenerator
        );

        // act
        _cleaner.Process(new OrnamentationCleaningItem(primaryNote, secondaryNote));

        // assert
        primaryNote.Should().BeEquivalentTo(expectedPrimaryNote);
        secondaryNote.Should().BeEquivalentTo(expectedSecondaryNote);
    }

    [Test]
    public void Process_does_not_clean_notes_when_ornamentations_do_not_conflict()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
        {
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth)
            }
        };

        var secondaryNote = new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.DoubleRun,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Sixteenth)
            }
        };

        var indicesToCheck = new[]
        {
            new NoteIndexPair(0, 1, 0),
            new NoteIndexPair(1, 3, 1),
            new NoteIndexPair(2, 5, 2)
        };

        _mockNotePairSelector.Select(Arg.Any<OrnamentationCleaningItem>()).Returns(new NotePair(primaryNote, secondaryNote));
        _mockNoteTargetSelector.Select(Arg.Any<OrnamentationCleaningItem>()).Returns(primaryNote);

        var expectedPrimaryNote = new BaroquenNote(primaryNote);
        var expectedSecondaryNote = new BaroquenNote(secondaryNote);

        _cleaner = new OrnamentationCleaner(
            new OrnamentationCleanerConfiguration(_mockNotePairSelector, indicesToCheck, _mockNoteTargetSelector),
            Configurations.GetCompositionConfiguration(2),
            _mockWeightedRandomBooleanGenerator
        );

        // act
        _cleaner.Process(new OrnamentationCleaningItem(primaryNote, secondaryNote));

        // assert
        primaryNote.Should().BeEquivalentTo(expectedPrimaryNote);
        secondaryNote.Should().BeEquivalentTo(expectedSecondaryNote);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Process_does_not_clean_when_conflicting_notes_do_not_occur_on_strong_pulse(bool cleanWeakPulseConflicts)
    {
        // arrange
        _mockWeightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(cleanWeakPulseConflicts);

        var primaryNote = new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.DoubleRun,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Sixteenth)
            }
        };

        var secondaryNote = new BaroquenNote(Instrument.Two, Notes.F4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.DoubleRun,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Sixteenth)
            }
        };

        var indicesToCheck = new[]
        {
            new NoteIndexPair(0, 0, 2),
            new NoteIndexPair(1, 1, 4),
            new NoteIndexPair(2, 2, 6),
            new NoteIndexPair(3, 3, 8),
            new NoteIndexPair(4, 4, 10),
            new NoteIndexPair(5, 5, 12),
            new NoteIndexPair(6, 6, 14)
        };

        _mockNotePairSelector.Select(Arg.Any<OrnamentationCleaningItem>()).Returns(new NotePair(primaryNote, secondaryNote));
        _mockNoteTargetSelector.Select(Arg.Any<OrnamentationCleaningItem>()).Returns(primaryNote);

        var expectedPrimaryNote = new BaroquenNote(primaryNote);
        var expectedSecondaryNote = new BaroquenNote(secondaryNote);

        _cleaner = new OrnamentationCleaner(
            new OrnamentationCleanerConfiguration(_mockNotePairSelector, indicesToCheck, _mockNoteTargetSelector),
            Configurations.GetCompositionConfiguration(2),
            _mockWeightedRandomBooleanGenerator
        );

        // act
        _cleaner.Process(new OrnamentationCleaningItem(primaryNote, secondaryNote));

        // assert
        primaryNote.Should().BeEquivalentTo(cleanWeakPulseConflicts ? new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half) : expectedPrimaryNote);
        secondaryNote.Should().BeEquivalentTo(expectedSecondaryNote);
    }
}

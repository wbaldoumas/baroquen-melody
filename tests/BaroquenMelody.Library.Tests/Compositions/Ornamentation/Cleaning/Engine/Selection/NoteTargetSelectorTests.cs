using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Selection;

[TestFixture]
internal sealed class NoteTargetSelectorTests
{
    private IOrnamentationCleaningSelectorStrategy _mockStrategy = null!;

    private NoteTargetSelector _noteTargetSelector = null!;

    [SetUp]
    public void SetUp()
    {
        _mockStrategy = Substitute.For<IOrnamentationCleaningSelectorStrategy>();

        _noteTargetSelector = new NoteTargetSelector([_mockStrategy]);
    }

    [Test]
    public void Select_returns_primary_note_when_strategy_selected_primary_note()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth);

        _mockStrategy.Select(primaryNote, secondaryNote).Returns(primaryNote);

        var ornamentationCleaningItem = new OrnamentationCleaningItem(primaryNote, secondaryNote);

        // act
        var result = _noteTargetSelector.Select(ornamentationCleaningItem);

        // assert
        result.Should().Be(primaryNote);
    }

    [Test]
    public void Select_returns_secondary_note_when_strategy_selected_secondary_note()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth);

        _mockStrategy.Select(primaryNote, secondaryNote).Returns(secondaryNote);

        var ornamentationCleaningItem = new OrnamentationCleaningItem(primaryNote, secondaryNote);

        // act
        var result = _noteTargetSelector.Select(ornamentationCleaningItem);

        // assert
        result.Should().Be(secondaryNote);
    }

    [Test]
    public void Select_throws_when_strategy_selected_null()
    {
        // arrange
        var primaryNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth);
        var secondaryNote = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth);

        _mockStrategy.Select(primaryNote, secondaryNote).Returns((BaroquenNote?)null);

        var ornamentationCleaningItem = new OrnamentationCleaningItem(primaryNote, secondaryNote);

        // act
        var act = () => _noteTargetSelector.Select(ornamentationCleaningItem);

        // assert
        act.Should().Throw<InvalidOperationException>();
    }
}

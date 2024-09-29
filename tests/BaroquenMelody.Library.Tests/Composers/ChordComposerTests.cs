using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

[TestFixture]
internal sealed class ChordComposerTests
{
    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ILogger _mockLogger = null!;

    private ChordComposer _chordComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockLogger = Substitute.For<ILogger>();

        _chordComposer = new ChordComposer(_mockCompositionStrategy, _mockLogger);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        var expectedChordA = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.D5, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G2, MusicalTimeSpan.Half)
        ]);

        var expectedChordB = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns(
        [
            expectedChordA,
            expectedChordB
        ]);

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        // act
        var resultChord = _chordComposer.Compose(precedingChords);

        // assert
        resultChord.Should().NotBeNull();
        resultChord.Should().Match<BaroquenChord>(actualChord => actualChord == expectedChordA || actualChord == expectedChordB);
    }

    [Test]
    public void WhenComposeIsInvoked_AndNoValidChordChoicesAreAvailable_ThenNoValidChordChoicesAvailableExceptionIsThrown()
    {
        // arrange
        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns(new List<ChordChoice>());

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        // act
        var act = () => _chordComposer.Compose(precedingChords);

        // assert
        act.Should().Throw<NoValidChordChoicesAvailableException>();
    }
}

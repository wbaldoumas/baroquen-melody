using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Scoring;
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

    private IChordSelector _mockChordSelector = null!;

    private ILogger _mockLogger = null!;

    private ChordComposer _chordComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockChordSelector = Substitute.For<IChordSelector>();
        _mockLogger = Substitute.For<ILogger>();

        _chordComposer = new ChordComposer(_mockCompositionStrategy, _mockChordSelector, _mockLogger);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenTheChordSelectorChoosesAmongThePossibleChords()
    {
        // arrange
        var possibleChordA = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.D5, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G2, MusicalTimeSpan.Half)
        ]);

        var possibleChordB = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
        ]);

        var possibleChords = new List<BaroquenChord> { possibleChordA, possibleChordB };

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        _mockCompositionStrategy.GetPossibleChords(precedingChords).Returns(possibleChords);
        _mockChordSelector.SelectNextChord(precedingChords, possibleChords).Returns(possibleChordB);

        // act
        var resultChord = _chordComposer.Compose(precedingChords);

        // assert
        resultChord.Should().BeSameAs(possibleChordB);
    }

    [Test]
    public void WhenComposeIsInvoked_AndNoValidChordChoicesAreAvailable_ThenNoValidChordChoicesAvailableExceptionIsThrown()
    {
        // arrange
        _mockChordSelector
            .SelectNextChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<IEnumerable<BaroquenChord>>())
            .Returns((BaroquenChord?)null);

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

    [Test]
    public void WhenComposeIsInvokedWithAPin_AndCandidatesHonorThePin_ThenTheSelectorChoosesAmongThePinnedCandidatesOnly()
    {
        // arrange - one candidate repeats the pinned voice's note, one moves it
        var pinHonoringChord = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
        ]);

        var pinBreakingChord = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        _mockCompositionStrategy.GetPossibleChords(precedingChords).Returns([pinBreakingChord, pinHonoringChord]);
        _mockChordSelector
            .SelectNextChord(precedingChords, Arg.Is<IEnumerable<BaroquenChord>>(candidates => candidates.SequenceEqual(new[] { pinHonoringChord })))
            .Returns(pinHonoringChord);

        // act
        var resultChord = _chordComposer.Compose(precedingChords, new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half));

        // assert - the selector received exactly the pin-honoring subset, and the candidate set was enumerated once
        resultChord.Should().BeSameAs(pinHonoringChord);
        _mockCompositionStrategy.Received(1).GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>());
    }

    [Test]
    public void WhenComposeIsInvokedWithAPin_AndNoCandidateHonorsThePin_ThenTheSelectorChoosesAmongTheFullCandidateSet()
    {
        // arrange - every candidate moves the pinned voice, so the pin degrades to the free choice
        var firstFreeChord = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        var secondFreeChord = new BaroquenChord(
        [
            new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half)
        ]);

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        _mockCompositionStrategy.GetPossibleChords(precedingChords).Returns([firstFreeChord, secondFreeChord]);
        _mockChordSelector
            .SelectNextChord(precedingChords, Arg.Is<IEnumerable<BaroquenChord>>(candidates => candidates.SequenceEqual(new[] { firstFreeChord, secondFreeChord })))
            .Returns(secondFreeChord);

        // act
        var resultChord = _chordComposer.Compose(precedingChords, new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half));

        // assert
        resultChord.Should().BeSameAs(secondFreeChord);
        _mockCompositionStrategy.Received(1).GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>());
    }

    [Test]
    public void WhenComposeIsInvokedWithAPin_AndTheCandidatesLackThePinnedVoice_ThenThePinDegradesInsteadOfThrowing()
    {
        // arrange - a candidate without the pinned voice must be skipped by the pin filter, not crash the walk
        var chordWithoutPinnedVoice = new BaroquenChord(
        [
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        _mockCompositionStrategy.GetPossibleChords(precedingChords).Returns([chordWithoutPinnedVoice]);
        _mockChordSelector
            .SelectNextChord(precedingChords, Arg.Is<IEnumerable<BaroquenChord>>(candidates => candidates.SequenceEqual(new[] { chordWithoutPinnedVoice })))
            .Returns(chordWithoutPinnedVoice);

        // act
        var resultChord = _chordComposer.Compose(precedingChords, new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half));

        // assert
        resultChord.Should().BeSameAs(chordWithoutPinnedVoice);
    }

    [Test]
    public void WhenComposeIsInvokedWithAPin_AndNoValidChordChoicesAreAvailable_ThenNoValidChordChoicesAvailableExceptionIsThrown()
    {
        // arrange
        _mockChordSelector
            .SelectNextChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<IEnumerable<BaroquenChord>>())
            .Returns((BaroquenChord?)null);

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        // act
        var act = () => _chordComposer.Compose(precedingChords, new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half));

        // assert
        act.Should().Throw<NoValidChordChoicesAvailableException>();
    }
}

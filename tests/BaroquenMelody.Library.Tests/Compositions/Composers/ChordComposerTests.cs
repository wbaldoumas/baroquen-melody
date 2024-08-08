using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Exceptions;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Composers;

[TestFixture]
internal sealed class ChordComposerTests
{
    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ILogger _mockLogger = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private ChordComposer _chordComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _chordComposer = new ChordComposer(_mockCompositionStrategy, _compositionConfiguration, _mockLogger);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns(
        [
            new ChordChoice(
            [
                new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 3),
                new NoteChoice(Voice.Alto, NoteMotion.Descending, 3)
            ]),
            new ChordChoice(
            [
                new NoteChoice(Voice.Soprano, NoteMotion.Descending, 3),
                new NoteChoice(Voice.Alto, NoteMotion.Ascending, 3)
            ])
        ]);

        var precedingChords = new List<BaroquenChord>
        {
            new(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        var expectedChordA = new BaroquenChord(
        [
            new BaroquenNote(Voice.Soprano, Notes.D5, MusicalTimeSpan.Half),
            new BaroquenNote(Voice.Alto, Notes.G2, MusicalTimeSpan.Half)
        ]);

        var expectedChordB = new BaroquenChord(
        [
            new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half),
            new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Half)
        ]);

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
                new BaroquenNote(Voice.Soprano, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half)
            ])
        };

        // act
        var act = () => _chordComposer.Compose(precedingChords);

        // assert
        act.Should().Throw<NoValidChordChoicesAvailableException>();
    }
}

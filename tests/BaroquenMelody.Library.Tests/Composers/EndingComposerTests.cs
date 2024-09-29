using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

[TestFixture]
internal sealed class EndingComposerTests
{
    private EndingComposer _endingComposer = null!;

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IChordNumberIdentifier _mockChordNumberIdentifier = null!;

    private IDispatcher _mockDispatcher = null!;

    private ILogger _mockLogger = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockCompositionDecorator = Substitute.For<ICompositionDecorator>();
        _mockChordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = TestCompositionConfigurations.Get();

        _mockChordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();

        _endingComposer = new EndingComposer(
            _mockCompositionStrategy,
            _mockCompositionDecorator,
            _mockChordNumberIdentifier,
            _mockDispatcher,
            _mockLogger,
            _compositionConfiguration
        );
    }

    [Test]
    public void Compose_GivenValidComposition_ReturnsComposedComposition()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Oblique, 1)])]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V, ChordNumber.V, ChordNumber.I);

        // act
        var result = _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Measures.Should().NotBeEmpty();
        _mockCompositionDecorator.Received(2).Decorate(Arg.Any<Composition>());
    }

    [Test]
    public void Compose_WhenNoChordChoicesAvailable_UsesFallbackChordChoice()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([], bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V, ChordNumber.V, ChordNumber.I);

        var fallbackChordChoice = new ChordChoice(
        [
            new NoteChoice(Instrument.One, NoteMotion.Oblique, 1),
            new NoteChoice(Instrument.Two, NoteMotion.Oblique, 1),
            new NoteChoice(Instrument.Three, NoteMotion.Oblique, 1),
            new NoteChoice(Instrument.Four, NoteMotion.Oblique, 1)
        ]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([fallbackChordChoice]);

        // act
        var result = _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Measures.Should().NotBeEmpty();
        _mockCompositionDecorator.Received(2).Decorate(Arg.Any<Composition>());
    }

    [Test]
    public void WhenMaxBridgingChordsAndMaxChordsToTonicAreReached_ThenCompositionIsStillReturned()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Oblique, 0)])]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V);

        // act
        var result = _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
    }

    private static Composition CreateTestComposition()
    {
        var measure = new Measure([new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))], Meter.FourFour);

        return new Composition([measure]);
    }

    private static BaroquenTheme CreateTestTheme()
    {
        var measure = new Measure([new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))], Meter.FourFour);

        return new BaroquenTheme([measure], [measure]);
    }
}

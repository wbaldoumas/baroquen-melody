using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Scoring;
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
internal sealed class ThemeComposerTests
{
    // mirrors ThemeComposer.MaxFugueCompositionAttempts, which bounds both the fugal and fallback retry loops.
    private const int MaxThemeCompositionAttempts = 50;

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IChordComposer _mockChordComposer = null!;

    private IFugalEntryPlacer _mockFugalEntryPlacer = null!;

    private IFugalAnswerStrategy _mockFugalAnswerStrategy = null!;

    private IDispatcher _mockDispatcher = null!;

    private ILogger _mockLogger = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private ThemeComposer _themeComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockCompositionDecorator = Substitute.For<ICompositionDecorator>();
        _mockChordComposer = Substitute.For<IChordComposer>();
        _mockFugalEntryPlacer = Substitute.For<IFugalEntryPlacer>();
        _mockFugalAnswerStrategy = Substitute.For<IFugalAnswerStrategy>();
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = TestCompositionConfigurations.Get();

        _themeComposer = new ThemeComposer(_mockCompositionStrategy, _mockCompositionDecorator, _mockChordComposer, _mockFugalEntryPlacer, _mockFugalAnswerStrategy, new WeightedChordSelector(new AggregateScoringRule([]), new ThreadLocalRandomProvider()), _mockDispatcher, _mockLogger, _compositionConfiguration);
    }

    [Test]
    public void WhenFugalThemeCannotBeComposed_FallbackBaroquenThemeIsReturned()
    {
        // arrange
        var mockBaroquenChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GenerateInitialChord().Returns(mockBaroquenChord);

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        _mockChordComposer.Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(new BaroquenChord(mockBaroquenChord));

        // act
        var result = _themeComposer.Compose(CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Exposition.Should().NotBeEmpty();
        result.Recapitulation.Should().NotBeEmpty();
    }

    [Test]
    public void WhenThemeCompositionDeadEnds_TheDeadEndIsRetriedAndAFallbackThemeIsReturned()
    {
        // arrange - every fugal attempt and the first fallback attempt dead-end with no valid chord choices;
        // the composition only completes if each dead end is retried rather than treated as fatal
        const int deadEndCount = MaxThemeCompositionAttempts + 1;

        var chord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GenerateInitialChord().Returns(chord);

        var composeCallCount = 0;

        _mockChordComposer.Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(_ => ++composeCallCount <= deadEndCount
                ? throw new NoValidChordChoicesAvailableException()
                : new BaroquenChord(chord));

        // act
        var result = _themeComposer.Compose(CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Exposition.Should().NotBeEmpty();
        result.Recapitulation.Should().NotBeEmpty();
    }

    [Test]
    public void WhenEveryThemeCompositionAttemptDeadEnds_TheDeadEndIsEventuallyFatal()
    {
        // arrange
        var chord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GenerateInitialChord().Returns(chord);

        _mockChordComposer.Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns<BaroquenChord>(static _ => throw new NoValidChordChoicesAvailableException());

        // act
        var act = () => _themeComposer.Compose(CancellationToken.None);

        // assert - every fugal attempt and every fallback attempt dead-ends on its first chord, so the
        // exception propagates only after both bounded retry loops are exhausted
        act.Should().Throw<NoValidChordChoicesAvailableException>();
        _mockChordComposer.Received(2 * MaxThemeCompositionAttempts).Compose(Arg.Any<IReadOnlyList<BaroquenChord>>());
    }

    [Test]
    public void Compose_uses_the_fugal_answer_strategy_for_alternating_answer_entries()
    {
        // arrange
        var chord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GenerateInitialChord().Returns(chord);
        _mockChordComposer.Compose(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns(_ => new BaroquenChord(chord));
        _mockFugalAnswerStrategy.GenerateAnswer(Arg.Any<IReadOnlyList<BaroquenNote>>()).Returns(static callInfo => callInfo.Arg<IReadOnlyList<BaroquenNote>>());

        // act
        _themeComposer.Compose(CancellationToken.None);

        // assert: the answer voice entries are derived through the fugal answer strategy.
        _mockFugalAnswerStrategy.Received().GenerateAnswer(Arg.Any<IReadOnlyList<BaroquenNote>>());
    }
}

using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
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
internal sealed class EndingComposerTests
{
    private EndingComposer _endingComposer = null!;

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IChordNumberIdentifier _mockChordNumberIdentifier = null!;

    private ICadenceClassifier _mockCadenceClassifier = null!;

    private ICadentialTrillApplicator _mockCadentialTrillApplicator = null!;

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
        _mockCadenceClassifier = Substitute.For<ICadenceClassifier>();
        _mockCadentialTrillApplicator = Substitute.For<ICadentialTrillApplicator>();

        _endingComposer = new EndingComposer(
            _mockCompositionStrategy,
            _mockCompositionDecorator,
            _mockChordNumberIdentifier,
            _mockCadenceClassifier,
            _mockCadentialTrillApplicator,
            new WeightedChordSelector(new AggregateScoringRule([]), new ThreadLocalRandomProvider()),
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
    public void Compose_WithDefaultScoringRulesEnabled_StillComposesTheFallbackBridgingChord()
    {
        // arrange: same fallback path as above, but the selector scores candidates with the default scoring rules,
        // exercising the materialize-and-score path through EndingComposer.GetNextChord.
        var realChordNumberIdentifier = new ChordNumberIdentifier(_compositionConfiguration);
        var scoringRuleFactory = new ScoringRuleFactory(
            _compositionConfiguration,
            realChordNumberIdentifier,
            new ChordInversionIdentifier(realChordNumberIdentifier, _compositionConfiguration));

        var endingComposer = new EndingComposer(
            _mockCompositionStrategy,
            _mockCompositionDecorator,
            _mockChordNumberIdentifier,
            _mockCadenceClassifier,
            _mockCadentialTrillApplicator,
            new WeightedChordSelector(scoringRuleFactory.CreateAggregate(AggregateScoringRuleConfiguration.Default), new ThreadLocalRandomProvider()),
            _mockDispatcher,
            _mockLogger,
            _compositionConfiguration
        );

        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([], bridgingChords);

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
        var result = endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Measures.Should().NotBeEmpty();
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

    [Test]
    public void Compose_WhenNoChordChoicesAreAvailable_StillComposesABestEffortEnding()
    {
        // arrange: the recapitulation is never reachable (no partially-voiced chords resolve) and no chord
        // choices exist anywhere, so both the bridge and the tonic hunt dead-end immediately - the ending
        // should degrade to a best-effort close instead of aborting the whole composition.
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([]);

        // act
        var result = _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert - the composition still closes with a resting chord
        result.Should().NotBeNull();
        result.Measures[^1].Beats[^1].Chord.Notes.Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.Rest);
    }

    [Test]
    public void Compose_AppliesTheCadentialTrillToThePenultimateAndFinalSoundingChords()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Oblique, 0)])]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V, ChordNumber.V, ChordNumber.I);

        BaroquenChord? capturedPenultimateChord = null;
        BaroquenChord? capturedFinalChord = null;

        _mockCadentialTrillApplicator
            .When(static applicator => applicator.ApplyTrill(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()))
            .Do(callInfo =>
            {
                capturedPenultimateChord = callInfo.ArgAt<BaroquenChord>(0);
                capturedFinalChord = callInfo.ArgAt<BaroquenChord>(1);
            });

        // act
        var result = _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert - the trill sees the final sounding chord (the one before the closing rest) and its predecessor
        _mockCadentialTrillApplicator.Received(1).ApplyTrill(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>());

        var allChords = result.Measures.SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)).ToList();

        allChords[^1].Notes.Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.Rest);
        capturedFinalChord.Should().BeSameAs(allChords[^2]);
        capturedPenultimateChord.Should().BeSameAs(allChords[^3]);
    }

    [Test]
    public void Compose_PrefersTheTonicVoicingFormingTheStrongestCadence()
    {
        // arrange - three tonic candidates are reachable; only the third forms a perfect authentic cadence
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        var stayingChoice = new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Oblique, 0)]);
        var ascendingChoice = new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Ascending, 1)]);
        var descendingChoice = new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Descending, 1)]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([stayingChoice, ascendingChoice, descendingChoice]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I, ChordNumber.I, ChordNumber.I);

        _mockCadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>())
            .Returns(CadenceType.None, CadenceType.None, CadenceType.PerfectAuthentic);

        BaroquenChord? capturedFinalChord = null;

        _mockCadentialTrillApplicator
            .When(static applicator => applicator.ApplyTrill(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()))
            .Do(callInfo => capturedFinalChord = callInfo.ArgAt<BaroquenChord>(1));

        var expectedFinalNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
            .ApplyNoteChoice(_compositionConfiguration.Scale, descendingChoice.NoteChoices[0], _compositionConfiguration.DefaultNoteTimeSpan);

        // act
        _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert
        _mockCadenceClassifier.Received(3).ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>());
        capturedFinalChord.Should().NotBeNull();
        capturedFinalChord![Instrument.One].Raw.Should().Be(expectedFinalNote.Raw);
    }

    [Test]
    public void Compose_DefersPlainTonicArrivalsWhileTheAuthenticCadenceBudgetLasts()
    {
        // arrange - a plain (non-authentic) tonic candidate is reachable at every step of the hunt; it should be
        // passed over until the authentic-cadence budget of twelve chords is exhausted, then accepted
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Oblique, 0)])]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I);

        _mockCadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>())
            .Returns(CadenceType.None);

        // act
        _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert - eleven budgeted rejections; once the budget expires the deferred arrival is accepted
        // without another classification
        _mockCadenceClassifier.Received(11).ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>());
    }

    [Test]
    public void Compose_WhenTheAuthenticCadenceBudgetExpires_RewindsToTheFirstDeferredTonicArrival()
    {
        // arrange - the first hunt step offers a plain tonic arrival, after which only non-tonic chords appear;
        // when the budget expires the ending should rewind to that first arrival rather than keep wandering
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        var ascendingChoice = new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Ascending, 1)]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([ascendingChoice]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I, ChordNumber.V);

        _mockCadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>())
            .Returns(CadenceType.None);

        BaroquenChord? capturedFinalChord = null;

        _mockCadentialTrillApplicator
            .When(static applicator => applicator.ApplyTrill(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()))
            .Do(callInfo => capturedFinalChord = callInfo.ArgAt<BaroquenChord>(1));

        var expectedFinalNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
            .ApplyNoteChoice(_compositionConfiguration.Scale, ascendingChoice.NoteChoices[0], _compositionConfiguration.DefaultNoteTimeSpan);

        // act
        _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert - the deferred arrival was classified once and becomes the final chord
        _mockCadenceClassifier.Received(1).ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>());
        capturedFinalChord.Should().NotBeNull();
        capturedFinalChord![Instrument.One].Raw.Should().Be(expectedFinalNote.Raw);
    }

    [Test]
    public void Compose_WhenTheTonicHuntDeadEnds_RewindsToTheFirstDeferredTonicArrival()
    {
        // arrange - the first hunt step offers a plain tonic arrival; the second offers no chord choices at
        // all, which previously aborted the composition and should now fall back to the deferred arrival
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        var ascendingChoice = new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Ascending, 1)]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([ascendingChoice], []);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I);

        _mockCadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>())
            .Returns(CadenceType.None);

        BaroquenChord? capturedFinalChord = null;

        _mockCadentialTrillApplicator
            .When(static applicator => applicator.ApplyTrill(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()))
            .Do(callInfo => capturedFinalChord = callInfo.ArgAt<BaroquenChord>(1));

        var expectedFinalNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
            .ApplyNoteChoice(_compositionConfiguration.Scale, ascendingChoice.NoteChoices[0], _compositionConfiguration.DefaultNoteTimeSpan);

        // act
        var result = _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        capturedFinalChord.Should().NotBeNull();
        capturedFinalChord![Instrument.One].Raw.Should().Be(expectedFinalNote.Raw);
    }

    [Test]
    public void Compose_StopsRankingTonicVoicingsOncePerfectAuthenticCadenceIsFound()
    {
        // arrange - the first tonic candidate already forms a perfect authentic cadence
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([
                new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Oblique, 0)]),
                new ChordChoice([new NoteChoice(Instrument.One, NoteMotion.Ascending, 1)])
            ]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I, ChordNumber.I);

        _mockCadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>())
            .Returns(CadenceType.PerfectAuthentic);

        // act
        _endingComposer.Compose(composition, theme, CancellationToken.None);

        // assert - the remaining tonic candidate is never classified
        _mockCadenceClassifier.Received(1).ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>());
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

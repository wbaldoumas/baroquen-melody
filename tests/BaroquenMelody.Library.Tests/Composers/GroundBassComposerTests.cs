using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Composers;

/// <summary>
///     The composer matrix: every structural obligation of the ground bass form is pinned against mocked
///     collaborators. The two-voice fixture gives the ground to Instrument.Two (the lower voice) with the
///     tetrachord rendered as C3-B2-A2-G2 over two statements of two measures each.
/// </summary>
[TestFixture]
internal sealed class GroundBassComposerTests
{
    private static readonly IReadOnlyList<Note> GroundNotes = [Notes.C3, Notes.B2, Notes.A2, Notes.G2];

    private CompositionConfiguration _configuration = null!;

    private GroundBassPlan _plan = null!;

    private IGroundBassPlanner _planner = null!;

    private ICompositionStrategy _strategy = null!;

    private ICompositionRule _rule = null!;

    private IChordSelector _selector = null!;

    private ICompositionDecorator _decorator = null!;

    private ISuspensionApplicator _suspensionApplicator = null!;

    private ITonicizationApplicator _tonicizationApplicator = null!;

    private ICadenceClassifier _cadenceClassifier = null!;

    private IChordNumberIdentifier _chordNumberIdentifier = null!;

    private ICadentialTrillApplicator _trillApplicator = null!;

    private IDynamicsApplicator _dynamicsApplicator = null!;

    private IComposer _fallbackComposer = null!;

    private IDispatcher _dispatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _configuration = TestCompositionConfigurations.Get(2, 4);
        _plan = new GroundBassPlan(
            GroundBassPattern.Bank[0],
            Instrument.Two,
            GroundNotes,
            StatementCount: 2,
            MeasuresPerStatement: 2,
            [new TonalSection(NoteName.C, Mode.Ionian, FirstStatement: 0, LastStatement: 1, GroundNotes)]);
        _planner = Substitute.For<IGroundBassPlanner>();
        _strategy = Substitute.For<ICompositionStrategy>();
        _rule = Substitute.For<ICompositionRule>();
        _selector = Substitute.For<IChordSelector>();
        _decorator = Substitute.For<ICompositionDecorator>();
        _suspensionApplicator = Substitute.For<ISuspensionApplicator>();
        _tonicizationApplicator = Substitute.For<ITonicizationApplicator>();
        _cadenceClassifier = Substitute.For<ICadenceClassifier>();
        _chordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();
        _trillApplicator = Substitute.For<ICadentialTrillApplicator>();
        _dynamicsApplicator = Substitute.For<IDynamicsApplicator>();
        _fallbackComposer = Substitute.For<IComposer>();
        _dispatcher = Substitute.For<IDispatcher>();

        _planner.CreatePlan().Returns(_plan);
        _rule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);
        _strategy.GenerateInitialChord().Returns(_ => BuildChord(Notes.E4, Notes.E3));
        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(call => new List<BaroquenChord> { HarmonizePin(call.ArgAt<BaroquenChord>(1)) });
        _strategy.HasPossibleChordForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);
        _strategy.GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns(_ => new List<BaroquenChord> { BuildChord(Notes.D4, Notes.D3) });
        _selector.SelectNextChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<IEnumerable<BaroquenChord>>())
            .Returns(call => call.Arg<IEnumerable<BaroquenChord>>().FirstOrDefault());
        _cadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()).Returns(CadenceType.PerfectAuthentic);
    }

    [Test]
    public void Compose_RendersTheGroundAtEveryOnsetWithAHeldDuplicateSlot()
    {
        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert: two statements of two measures plus the final cadence measure.
        composition.Measures.Should().HaveCount(5);

        var expectedBassLine = GroundNotes.Concat(GroundNotes).ToList();
        var statementChords = composition.Measures.Take(4).SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)).ToList();

        statementChords.Should().HaveCount(16, "each ground note spans an onset slot and a held slot");

        for (var chordIndex = 0; chordIndex < statementChords.Count; ++chordIndex)
        {
            statementChords[chordIndex][Instrument.Two].Raw.Should().Be(expectedBassLine[chordIndex / GroundBassPlan.SlotsPerGroundNote], $"slot {chordIndex} must carry the ground");
        }
    }

    [Test]
    public void Compose_StripsTheOpeningStatementToTheBassAlone()
    {
        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        foreach (var chord in composition.Measures.Take(2).SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)))
        {
            chord.Notes.Should().ContainSingle("the announcement is the ground alone").Which.Instrument.Should().Be(Instrument.Two);
        }

        foreach (var chord in composition.Measures.Skip(2).Take(2).SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)))
        {
            chord.ContainsInstrument(Instrument.One).Should().BeTrue("the full texture enters with the second statement");
        }
    }

    [Test]
    public void Compose_PinsTheGroundsFirstNoteIntoTheOpeningVoicing()
    {
        // arrange: the initial voicing generator proposes E3 in the bass, which the boot must replace.
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Measures[0].Beats[0].Chord[Instrument.Two].Raw.Should().Be(Notes.C3);
    }

    [Test]
    public void Compose_WhenNoOpeningVoicingSatisfiesTheRules_DegradesToTheLastCandidate()
    {
        // arrange
        _rule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(false);

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        _strategy.Received(50).GenerateInitialChord();
        composition.Measures[0].Beats[0].Chord[Instrument.Two].Raw.Should().Be(Notes.C3, "the degraded voicing still carries the ground");
    }

    [Test]
    public void Compose_WhenNoGroundFitsTheBassRange_FallsBackToTheStandardComposer()
    {
        // arrange
        var fallbackComposition = new Composition([]);

        _planner.CreatePlan().Returns((GroundBassPlan?)null);
        _fallbackComposer.Compose(Arg.Any<CancellationToken>()).Returns(fallbackComposition);

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Should().BeSameAs(fallbackComposition);
        _strategy.DidNotReceive().GenerateInitialChord();
    }

    [Test]
    public void Compose_WhenAnOnsetDeadEnds_RetriesTheWholeCompositionFromFreshDraws()
    {
        // arrange: the very first pinned onset finds no candidate once, then the walk heals.
        var pinnedCalls = 0;

        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(call => ++pinnedCalls == 1 ? [] : new List<BaroquenChord> { HarmonizePin(call.ArgAt<BaroquenChord>(1)) });

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Measures.Should().HaveCount(5);
        _strategy.Received(2).GenerateInitialChord();
        _fallbackComposer.DidNotReceive().Compose(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Compose_WhenAnOnsetAlwaysStarves_TakesTheLocalLibertyOnTheFinalAttempt()
    {
        // arrange: no pinned candidate ever exists, so every attempt dead-ends until the final attempt
        // composes the onsets unpinned.
        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Measures.Should().HaveCount(5);
        composition.Measures[2].Beats[0].Chord[Instrument.Two].Raw.Should().Be(Notes.D3, "the starving onset composes free of the pin");
        _strategy.Received(10).GenerateInitialChord();
        _fallbackComposer.DidNotReceive().Compose(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Compose_WhenEvenTheFreeWalkIsEmpty_FallsBackToTheStandardComposer()
    {
        // arrange
        var fallbackComposition = new Composition([]);

        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns([]);
        _strategy.GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns([]);
        _fallbackComposer.Compose(Arg.Any<CancellationToken>()).Returns(fallbackComposition);

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Should().BeSameAs(fallbackComposition);
    }

    [Test]
    public void Compose_ClosesWithTheStrongestReachableCadence()
    {
        // arrange: the pinned-tonic candidates arrive weakest-first, so only the rank filter can pick the
        // authentic close over the selector's first-candidate habit.
        var plagalClose = BuildChord(Notes.F4, Notes.C3);
        var authenticClose = BuildChord(Notes.E4, Notes.C3);

        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Is<BaroquenChord>(static pin => pin.Notes.Count == 1 && pin[Instrument.Two].Raw == Notes.C3))
            .Returns(call => new List<BaroquenChord> { HarmonizePin(call.ArgAt<BaroquenChord>(1)), plagalClose, authenticClose });
        _cadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()).Returns(CadenceType.Plagal);
        _cadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), authenticClose).Returns(CadenceType.PerfectAuthentic);

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Measures[^1].Beats[0].Chord.Should().BeSameAs(authenticClose);
        _trillApplicator.Received(1).ApplyTrill(Arg.Is<BaroquenChord>(static chord => chord[Instrument.Two].Raw == Notes.G2), authenticClose);
    }

    [Test]
    public void Compose_WhenTheCloseHasNoCandidatesAtAll_DuplicatesTheLastChordStrippedOfItsOrnamentation()
    {
        // arrange: the walk itself stays healthy, but the closing onset finds no pinned candidate and the
        // free walk is empty too - the close must degrade to holding the last statement chord. The
        // decorator ornaments the walk's chords before the close, so the duplicate must be reset: copied
        // sub-notes would otherwise render after the stretched whole note and desynchronize the voice.
        var pinnedCalls = 0;

        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(call => ++pinnedCalls == 8 ? [] : new List<BaroquenChord> { HarmonizePin(call.ArgAt<BaroquenChord>(1)) });
        _strategy.GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns([]);
        _decorator.When(static decorator => decorator.Decorate(Arg.Any<Composition>(), Instrument.One))
            .Do(call =>
            {
                var upperNote = call.Arg<Composition>().Measures[^1].Beats[^1].Chord[Instrument.One];

                upperNote.OrnamentationType = OrnamentationType.Mordent;
                upperNote.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Quarter));
            });

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Measures.Should().HaveCount(5);
        composition.Measures[^1].Beats[0].Chord[Instrument.Two].Raw.Should().Be(Notes.G2, "the degraded close holds the last statement chord instead of arriving on the tonic");
        composition.Measures[^1].Beats[0].Chord.Notes.Should().OnlyContain(
            static note => note.OrnamentationType == OrnamentationType.None && note.Ornamentations.Count == 0,
            "the duplicated close must shed the copied ornamentation before stretching to a whole note");
    }

    [Test]
    public void Compose_WhenTheSelectorDeclinesTheClose_TakesTheStrongestCandidateDirectly()
    {
        // arrange: the walk's seven onset selections succeed; the eighth selection is the close, which the
        // selector declines, so the composer takes the strongest-ranked candidate itself.
        var selectorCalls = 0;

        _selector.SelectNextChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<IEnumerable<BaroquenChord>>())
            .Returns(call => ++selectorCalls == 8 ? null : call.Arg<IEnumerable<BaroquenChord>>().FirstOrDefault());

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        composition.Measures.Should().HaveCount(5);
        composition.Measures[^1].Beats[0].Chord[Instrument.Two].Raw.Should().Be(Notes.C3, "the strongest pinned-tonic candidate closes the piece");
    }

    [Test]
    public void Compose_StretchesTheFinalChordAndAppendsARestingChord()
    {
        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        var finalMeasure = composition.Measures[^1];

        finalMeasure.Beats.Should().HaveCount(2);
        finalMeasure.Beats[0].Chord.Notes.Should().OnlyContain(static note => note.MusicalTimeSpan == MusicalTimeSpan.Whole);
        finalMeasure.Beats[1].Chord.Notes.Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.Rest);
    }

    [Test]
    public void Compose_DecoratesOnlyTheUpperVoicesAndOnlyFromTheSecondStatement()
    {
        // act
        _ = CreateComposer().Compose(CancellationToken.None);

        // assert
        _decorator.Received(1).Decorate(Arg.Is<Composition>(static composition => composition.Measures.Count == 2), Instrument.One);
        _decorator.DidNotReceive().Decorate(Arg.Any<Composition>(), Instrument.Two);
    }

    [Test]
    public void Compose_RunsTheHarmonicPassesOverTheTrailingStatementsOnly()
    {
        // arrange
        Composition? suspensionTarget = null;
        Composition? tonicizationTarget = null;

        _suspensionApplicator.When(static applicator => applicator.ApplySuspensions(Arg.Any<Composition>()))
            .Do(call => suspensionTarget = call.Arg<Composition>());
        _tonicizationApplicator.When(static applicator => applicator.ApplyTonicization(Arg.Any<Composition>()))
            .Do(call => tonicizationTarget = call.Arg<Composition>());

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert: the passes see the trailing statements and the final measure, sharing measure references
        // with the full composition, while the solo announcement stays out of reach.
        suspensionTarget.Should().NotBeNull();
        suspensionTarget!.Measures.Should().HaveCount(3);
        suspensionTarget.Measures[0].Should().BeSameAs(composition.Measures[2]);
        tonicizationTarget.Should().NotBeNull();
        tonicizationTarget!.Measures.Should().HaveCount(3);
        _decorator.Received(1).ApplySustain(Arg.Is<Composition>(static fullComposition => fullComposition.Measures.Count == 5));
        _dynamicsApplicator.Received(1).Apply(Arg.Is<Composition>(static fullComposition => fullComposition.Measures.Count == 5));
    }

    [Test]
    public void Compose_DispatchesTheCompositionStepsInPipelineOrder()
    {
        // act
        _ = CreateComposer().Compose(CancellationToken.None);

        // assert
        Received.InOrder(() =>
        {
            _dispatcher.Dispatch(Arg.Any<ResetCompositionProgress>());
            _dispatcher.Dispatch(Arg.Is<ProgressCompositionStep>(static step => step.Step == CompositionStep.Theme));
            _dispatcher.Dispatch(Arg.Is<ProgressCompositionStep>(static step => step.Step == CompositionStep.Body));
            _dispatcher.Dispatch(Arg.Is<ProgressCompositionStep>(static step => step.Step == CompositionStep.Ornamentation));
            _dispatcher.Dispatch(Arg.Is<ProgressCompositionStep>(static step => step.Step == CompositionStep.Ending));
            _dispatcher.Dispatch(Arg.Is<ProgressCompositionEndingProgress>(static progress => progress.Progress == 100));
            _dispatcher.Dispatch(Arg.Is<ProgressCompositionStep>(static step => step.Step == CompositionStep.Complete));
        });
    }

    [Test]
    public void Compose_CompletesEveryProgressChannelTheOverallBarAverages()
    {
        // The overall progress bar is the average of the theme, body, and ending progress channels, so
        // each must report 100 or the bar freezes short of complete - found in the field at 67% when the
        // ending channel was never dispatched.
        _ = CreateComposer().Compose(CancellationToken.None);

        _dispatcher.Received(1).Dispatch(Arg.Is<ProgressCompositionThemeProgress>(static progress => progress.Progress == 100));
        _dispatcher.Received().Dispatch(Arg.Is<ProgressCompositionBodyProgress>(static progress => progress.Progress == 100));
        _dispatcher.Received(1).Dispatch(Arg.Is<ProgressCompositionEndingProgress>(static progress => progress.Progress == 100));
    }

    [Test]
    public void Compose_WhenNoCadenceClassifies_PrefersATonicQualityFinalChord()
    {
        // arrange: the seam's chord pair classifies as no cadence at all, so the authentic ranks are out of
        // reach and every pinned-tonic arrival would tie at the plain rank - where the selector's first pick
        // is the submediant color (A4 over the tonic bass). The close must instead prefer the arrival that
        // parses as the tonic harmony, leaving the submediant as a true last resort.
        _cadenceClassifier.ClassifyCadence(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>()).Returns(CadenceType.None);
        _chordNumberIdentifier.IdentifyChordNumber(Arg.Is<BaroquenChord>(chord => chord[Instrument.One].Raw == Notes.G4)).Returns(ChordNumber.I);
        _chordNumberIdentifier.IdentifyChordNumber(Arg.Is<BaroquenChord>(chord => chord[Instrument.One].Raw == Notes.A4)).Returns(ChordNumber.VI);
        _strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(call => new List<BaroquenChord>
            {
                HarmonizePinWith(call.ArgAt<BaroquenChord>(1), Notes.A4),
                HarmonizePinWith(call.ArgAt<BaroquenChord>(1), Notes.G4)
            });

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        var finalChord = composition.Measures[^1].Beats[0].Chord;

        finalChord[Instrument.Two].Raw.Should().Be(Notes.C3, "the close always lands the ground's tonic in the bass");
        finalChord[Instrument.One].Raw.Should().Be(Notes.G4, "a tonic-quality arrival must outrank the submediant color when no cadence classifies");
    }

    [Test]
    public void Compose_WithACancelledToken_Throws()
    {
        // arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // act
        var act = () => CreateComposer().Compose(cancellationTokenSource.Token);

        // assert
        act.Should().Throw<OperationCanceledException>();
    }

    private GroundBassComposer CreateComposer() => new(
        _planner,
        _strategy,
        _rule,
        _selector,
        _decorator,
        _suspensionApplicator,
        _tonicizationApplicator,
        _cadenceClassifier,
        _chordNumberIdentifier,
        _trillApplicator,
        _dynamicsApplicator,
        _fallbackComposer,
        _dispatcher,
        Substitute.For<ILogger>(),
        _configuration
    );

    private static BaroquenChord BuildChord(Note upperNote, Note bassNote) => new(
    [
        new BaroquenNote(Instrument.One, upperNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, bassNote, MusicalTimeSpan.Half)
    ]);

    private static BaroquenChord HarmonizePin(BaroquenChord pinnedChord) => HarmonizePinWith(pinnedChord, Notes.G4);

    private static BaroquenChord HarmonizePinWith(BaroquenChord pinnedChord, Note upperNote) => new(
    [
        new BaroquenNote(Instrument.One, upperNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, pinnedChord[Instrument.Two].Raw, MusicalTimeSpan.Half)
    ]);
}

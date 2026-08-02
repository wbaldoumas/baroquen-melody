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
using BaroquenMelody.Library.Rhythm;
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

    private static readonly IReadOnlyList<Note> ForeignGroundNotes = [Notes.A2, Notes.G2, Notes.F2, Notes.E2];

    private CompositionConfiguration _configuration = null!;

    private GroundBassPlan _plan = null!;

    private GroundBassPlan _modulatingPlan = null!;

    private IGroundBassPlanner _planner = null!;

    private ICompositionStrategy _strategy = null!;

    private ICompositionStrategy _homeSeamStrategy = null!;

    private ICompositionStrategy _relativeStrategy = null!;

    private ICompositionStrategy _relativeSeamStrategy = null!;

    private ICompositionRule _rule = null!;

    private IChordSelector _selector = null!;

    private IChordSelector _relativeSelector = null!;

    private ICompositionDecorator _decorator = null!;

    private ICompositionDecorator _relativeDecorator = null!;

    private ISuspensionApplicator _suspensionApplicator = null!;

    private ITonicizationApplicator _tonicizationApplicator = null!;

    private ITonicizationApplicator _relativeTonicizationApplicator = null!;

    private IGroundBassDivisionScheduler _divisionScheduler = null!;

    private VoiceRhythmLedger _voiceRhythmLedger = null!;

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
        _modulatingPlan = new GroundBassPlan(
            GroundBassPattern.Bank[0],
            Instrument.Two,
            GroundNotes,
            StatementCount: 4,
            MeasuresPerStatement: 2,
            [
                new TonalSection(NoteName.C, Mode.Ionian, FirstStatement: 0, LastStatement: 1, GroundNotes),
                new TonalSection(NoteName.A, Mode.Aeolian, FirstStatement: 2, LastStatement: 2, ForeignGroundNotes),
                new TonalSection(NoteName.C, Mode.Ionian, FirstStatement: 3, LastStatement: 3, GroundNotes)
            ]);
        _planner = Substitute.For<IGroundBassPlanner>();
        _strategy = Substitute.For<ICompositionStrategy>();
        _homeSeamStrategy = Substitute.For<ICompositionStrategy>();
        _relativeStrategy = Substitute.For<ICompositionStrategy>();
        _relativeSeamStrategy = Substitute.For<ICompositionStrategy>();
        _rule = Substitute.For<ICompositionRule>();
        _selector = Substitute.For<IChordSelector>();
        _relativeSelector = Substitute.For<IChordSelector>();
        _decorator = Substitute.For<ICompositionDecorator>();
        _relativeDecorator = Substitute.For<ICompositionDecorator>();
        _suspensionApplicator = Substitute.For<ISuspensionApplicator>();
        _tonicizationApplicator = Substitute.For<ITonicizationApplicator>();
        _relativeTonicizationApplicator = Substitute.For<ITonicizationApplicator>();
        _divisionScheduler = Substitute.For<IGroundBassDivisionScheduler>();
        _voiceRhythmLedger = new VoiceRhythmLedger();
        _cadenceClassifier = Substitute.For<ICadenceClassifier>();
        _chordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();
        _trillApplicator = Substitute.For<ICadentialTrillApplicator>();
        _dynamicsApplicator = Substitute.For<IDynamicsApplicator>();
        _fallbackComposer = Substitute.For<IComposer>();
        _dispatcher = Substitute.For<IDispatcher>();

        _planner.CreatePlan().Returns(_plan);
        _rule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);
        _strategy.GenerateInitialChord().Returns(_ => BuildChord(Notes.E4, Notes.E3));

        foreach (var strategy in new[] { _strategy, _homeSeamStrategy, _relativeStrategy, _relativeSeamStrategy })
        {
            strategy.GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
                .Returns(call => new List<BaroquenChord> { HarmonizePin(call.ArgAt<BaroquenChord>(1)) });
            strategy.HasPossibleChordForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);
            strategy.GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>()).Returns(_ => new List<BaroquenChord> { BuildChord(Notes.D4, Notes.D3) });
        }

        foreach (var selector in new[] { _selector, _relativeSelector })
        {
            selector.SelectNextChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<IEnumerable<BaroquenChord>>())
                .Returns(call => call.Arg<IEnumerable<BaroquenChord>>().FirstOrDefault());
        }

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
    public void Compose_WithAModulatingPlan_ComposesEachSectionUnderItsKeyComponents()
    {
        // arrange: four statements plan as home, home, relative, home - so the walk crosses a key seam
        // twice. The seam transitions belong to the ARRIVING key: entering the relative section validates
        // under the relative seam strategy, returning home validates under the home seam strategy, and the
        // sections' interiors use their full strategies and selectors.
        var composition = CreateModulatingComposer().Compose(CancellationToken.None);

        // assert: the bass renders each statement from its section's rendering.
        var statementChords = composition.Measures.Take(8).SelectMany(static measure => measure.Beats.Select(static beat => beat.Chord)).ToList();
        var expectedBassLine = GroundNotes.Concat(GroundNotes).Concat(ForeignGroundNotes).Concat(GroundNotes).ToList();

        for (var chordIndex = 0; chordIndex < statementChords.Count; ++chordIndex)
        {
            statementChords[chordIndex][Instrument.Two].Raw.Should().Be(expectedBassLine[chordIndex / GroundBassPlan.SlotsPerGroundNote], $"slot {chordIndex} must carry its section's rendering of the ground");
        }

        // The relative seam strategy owns exactly the one onset entering the relative section (pinned A2)
        // and the thread-check crossing into it; the home seam strategy owns exactly the return onset
        // (pinned C3) and its crossing thread-check; the relative full strategy owns the section's interior.
        _relativeSeamStrategy.Received(1).GetRuleValidChordsForPartiallyVoicedChord(
            Arg.Any<IReadOnlyList<BaroquenChord>>(),
            Arg.Is<BaroquenChord>(static pin => pin.Notes.Count == 1 && pin[Instrument.Two].Raw == Notes.A2));
        _relativeSeamStrategy.Received(1).HasPossibleChordForPartiallyVoicedChord(
            Arg.Any<IReadOnlyList<BaroquenChord>>(),
            Arg.Is<BaroquenChord>(static pin => pin.Notes.Count == 1 && pin[Instrument.Two].Raw == Notes.A2));
        _relativeStrategy.Received(3).GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>());
        _homeSeamStrategy.Received(1).GetRuleValidChordsForPartiallyVoicedChord(
            Arg.Any<IReadOnlyList<BaroquenChord>>(),
            Arg.Is<BaroquenChord>(static pin => pin.Notes.Count == 1 && pin[Instrument.Two].Raw == Notes.C3));
        _homeSeamStrategy.Received(1).HasPossibleChordForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>());
        _relativeSelector.Received(4).SelectNextChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<IEnumerable<BaroquenChord>>());
        _relativeStrategy.DidNotReceive().GenerateInitialChord();
    }

    [Test]
    public void Compose_WithAModulatingPlan_DecoratesEachSectionSliceWithItsOwnDecorator()
    {
        // arrange
        var homeDecoratedSlices = new List<Composition>();
        var relativeDecoratedSlices = new List<Composition>();

        _decorator.When(static decorator => decorator.Decorate(Arg.Any<Composition>(), Instrument.One))
            .Do(call => homeDecoratedSlices.Add(call.Arg<Composition>()));
        _relativeDecorator.When(static decorator => decorator.Decorate(Arg.Any<Composition>(), Instrument.One))
            .Do(call => relativeDecoratedSlices.Add(call.Arg<Composition>()));

        // act
        var composition = CreateModulatingComposer().Compose(CancellationToken.None);

        // assert: the home decorator sees the lead statement after the solo (measures 2-3) and the return
        // statement (measures 6-7); the relative decorator sees the relative statement (measures 4-5). The
        // slices share measure references with the composition, and the bass voice is never decorated.
        homeDecoratedSlices.Should().HaveCount(2);
        homeDecoratedSlices[0].Measures.Should().HaveCount(2).And.ContainInOrder(composition.Measures[2], composition.Measures[3]);
        homeDecoratedSlices[1].Measures.Should().HaveCount(2).And.ContainInOrder(composition.Measures[6], composition.Measures[7]);
        relativeDecoratedSlices.Should().ContainSingle();
        relativeDecoratedSlices[0].Measures.Should().HaveCount(2).And.ContainInOrder(composition.Measures[4], composition.Measures[5]);
        _decorator.DidNotReceive().Decorate(Arg.Any<Composition>(), Instrument.Two);
        _relativeDecorator.DidNotReceive().Decorate(Arg.Any<Composition>(), Instrument.Two);
    }

    [Test]
    public void Compose_WithAModulatingPlan_AppliesEachSectionsTonicizationOverItsSliceInOrder()
    {
        // arrange
        var homeTonicizedSlices = new List<Composition>();
        var relativeTonicizedSlices = new List<Composition>();
        Composition? suspensionTarget = null;

        _tonicizationApplicator.When(static applicator => applicator.ApplyTonicization(Arg.Any<Composition>()))
            .Do(call =>
            {
                if (suspensionTarget is null)
                {
                    throw new InvalidOperationException("suspensions must run before any tonicization slice");
                }

                homeTonicizedSlices.Add(call.Arg<Composition>());
            });
        _relativeTonicizationApplicator.When(static applicator => applicator.ApplyTonicization(Arg.Any<Composition>()))
            .Do(call =>
            {
                if (suspensionTarget is null)
                {
                    throw new InvalidOperationException("suspensions must run before any tonicization slice");
                }

                relativeTonicizedSlices.Add(call.Arg<Composition>());
            });
        _suspensionApplicator.When(static applicator => applicator.ApplySuspensions(Arg.Any<Composition>()))
            .Do(call => suspensionTarget = call.Arg<Composition>());

        // act
        var composition = CreateModulatingComposer().Compose(CancellationToken.None);

        // assert: suspensions still run once over the whole trailing sub-composition (its diatonic-step
        // eligibility reads identically in relative keys), while tonicization runs per section slice - the
        // home lead after the solo, the relative middle, and the home tail carrying the appended close.
        // Every non-final slice carries the next section's first measure as a read-toward boundary, so
        // slice-final measures keep their mid-measure sites and the cross-key seam pairs are real sites
        // under the departing key's licenses.
        suspensionTarget.Should().NotBeNull();
        suspensionTarget!.Measures.Should().HaveCount(7, "the trailing sub-composition spans every statement after the solo plus the close");
        suspensionTarget.Measures[0].Should().BeSameAs(composition.Measures[2]);
        homeTonicizedSlices.Should().HaveCount(2);
        homeTonicizedSlices[0].Measures.Should().HaveCount(3).And.ContainInOrder(composition.Measures[2], composition.Measures[3], composition.Measures[4]);
        homeTonicizedSlices[1].Measures.Should().HaveCount(3).And.ContainInOrder(composition.Measures[6], composition.Measures[7], composition.Measures[8]);
        relativeTonicizedSlices.Should().ContainSingle();
        relativeTonicizedSlices[0].Measures.Should().HaveCount(3).And.ContainInOrder(composition.Measures[4], composition.Measures[5], composition.Measures[6]);
        _decorator.Received(1).ApplySustain(Arg.Is<Composition>(static fullComposition => fullComposition.Measures.Count == 9));
    }

    [Test]
    public void Compose_WithASoloOnlyLeadSection_SkipsItsEmptyDecorationSlice()
    {
        // arrange: the planner's lead floor keeps two statements home, but the composer must tolerate any
        // plan shape - a lead holding only the solo announcement yields an empty texture slice, which must
        // be skipped rather than decorated. Three statements: solo home, foreign, home.
        var soloLeadPlan = new GroundBassPlan(
            GroundBassPattern.Bank[0],
            Instrument.Two,
            GroundNotes,
            StatementCount: 3,
            MeasuresPerStatement: 2,
            [
                new TonalSection(NoteName.C, Mode.Ionian, FirstStatement: 0, LastStatement: 0, GroundNotes),
                new TonalSection(NoteName.A, Mode.Aeolian, FirstStatement: 1, LastStatement: 1, ForeignGroundNotes),
                new TonalSection(NoteName.C, Mode.Ionian, FirstStatement: 2, LastStatement: 2, GroundNotes)
            ]);

        _planner.CreatePlan().Returns(soloLeadPlan);

        var homeDecoratedSlices = new List<Composition>();

        _decorator.When(static decorator => decorator.Decorate(Arg.Any<Composition>(), Instrument.One))
            .Do(call => homeDecoratedSlices.Add(call.Arg<Composition>()));

        // act
        var composition = CreateComposer(new GroundBassSectionComponents(_relativeStrategy, _relativeSeamStrategy, _relativeSelector, _relativeDecorator, _relativeTonicizationApplicator))
            .Compose(CancellationToken.None);

        // assert: the home decorator sees only the return statement (measures 4-5); the solo-only lead
        // contributes no slice at all.
        homeDecoratedSlices.Should().ContainSingle();
        homeDecoratedSlices[0].Measures.Should().HaveCount(2).And.ContainInOrder(composition.Measures[4], composition.Measures[5]);
    }

    [Test]
    public void Compose_WithAForeignSectionButNoRelativeComponents_Throws()
    {
        // arrange: the configurator always builds relative components when a foreign section can be
        // planned, so reaching a foreign section without them is a wiring defect that must fail loudly
        // rather than compose the relative section under the home key's grammar.
        _planner.CreatePlan().Returns(_modulatingPlan);

        // act
        var act = () => CreateComposer(relativeComponents: null).Compose(CancellationToken.None);

        // assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*without relative-key components*");
    }

    [Test]
    public void Compose_RecordsDivisionRoles_ForTheWinningWalk()
    {
        // arrange: the scheduler holds the ground line, escalates statement 1 at 140, and gives its
        // figuration to Instrument.One. The announcement can record only the ground line (the strip has
        // discarded its upper voices), and the close, composed after the recording site, is never recorded.
        _divisionScheduler.ShouldHoldGroundLine().Returns(true);
        _divisionScheduler.TryGetIntensity(Arg.Any<GroundBassPlan>(), 1, out Arg.Any<int>())
            .Returns(static callInfo =>
            {
                callInfo[2] = 140;

                return true;
            });
        _divisionScheduler.TryGetFloridInstrument(Arg.Any<GroundBassPlan>(), 1, out Arg.Any<Instrument>())
            .Returns(static callInfo =>
            {
                callInfo[2] = Instrument.One;

                return true;
            });

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        var announcementBassNotes = composition.Measures.Take(2).SelectMany(static measure => measure.Beats).Select(static beat => beat.Chord[Instrument.Two]).ToList();

        announcementBassNotes.Should().OnlyContain(note => _voiceRhythmLedger.IsHeldNote(note), "the ground line holds through the announcement");

        var statementBeats = composition.Measures.Skip(2).Take(2).SelectMany(static measure => measure.Beats).ToList();

        statementBeats.Select(static beat => beat.Chord[Instrument.Two]).Should().OnlyContain(note => _voiceRhythmLedger.IsHeldNote(note), "the ground line holds through every statement");

        foreach (var upperNote in statementBeats.Select(static beat => beat.Chord[Instrument.One]))
        {
            _voiceRhythmLedger.TryGetDivisionIntensity(upperNote, out var intensity).Should().BeTrue("every accompanied upper note carries its statement's intensity");
            intensity.Should().Be(140);
            _voiceRhythmLedger.IsFloridNote(upperNote).Should().BeTrue("statement 1's florid voice is Instrument.One");
        }

        foreach (var closeNote in composition.Measures[^1].Beats.SelectMany(static beat => beat.Chord.Notes))
        {
            _voiceRhythmLedger.IsHeldNote(closeNote).Should().BeFalse("the close is composed after the recording site");
            _voiceRhythmLedger.TryGetDivisionIntensity(closeNote, out _).Should().BeFalse("the close never escalates");
        }
    }

    [Test]
    public void Compose_WhenAStatementEscalatesWithoutFiguration_RecordsIntensityAlone()
    {
        // arrange: the production scheduler answers the intensity and figuration queries from one gate,
        // but the contract keeps them independent - a scheduler may carry a statement's intensity without
        // rotating a florid voice onto it, and the recorder must not conflate the two answers.
        _divisionScheduler.ShouldHoldGroundLine().Returns(true);
        _divisionScheduler.TryGetIntensity(Arg.Any<GroundBassPlan>(), 1, out Arg.Any<int>())
            .Returns(static callInfo =>
            {
                callInfo[2] = 90;

                return true;
            });

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        foreach (var upperNote in composition.Measures.Skip(2).Take(2).SelectMany(static measure => measure.Beats).Select(static beat => beat.Chord[Instrument.One]))
        {
            _voiceRhythmLedger.TryGetDivisionIntensity(upperNote, out var intensity).Should().BeTrue("the statement's intensity stands without a florid voice");
            intensity.Should().Be(90);
            _voiceRhythmLedger.IsFloridNote(upperNote).Should().BeFalse("no florid voice was scheduled for the statement");
        }
    }

    [Test]
    public void Compose_WhenTheSchedulerDeclines_RecordsNothingAndKeepsVelocities()
    {
        // arrange: the substitute's default false answers are the divisions-off shape

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        foreach (var note in composition.Measures.SelectMany(static measure => measure.Beats).SelectMany(static beat => beat.Chord.Notes))
        {
            _voiceRhythmLedger.IsHeldNote(note).Should().BeFalse();
            _voiceRhythmLedger.IsFloridNote(note).Should().BeFalse();
            _voiceRhythmLedger.TryGetDivisionIntensity(note, out _).Should().BeFalse();
            ((int)note.Velocity).Should().Be(75, "no terrace may touch a declining composition's velocities");
        }
    }

    [Test]
    public void Compose_AppliesTheTerraceExactlyOncePerNote_WithClamping()
    {
        // arrange: statement 1 terraces by -10 from the default 75-velocity notes; the instrument ceiling
        // is 60, so a single application clamps to the ceiling while a double application would land at 50
        // - the assertion distinguishes exactly-once from compounding.
        _divisionScheduler.TryGetTerraceOffset(Arg.Any<GroundBassPlan>(), 1, out Arg.Any<int>())
            .Returns(static callInfo =>
            {
                callInfo[2] = -10;

                return true;
            });

        // act
        var composition = CreateComposer().Compose(CancellationToken.None);

        // assert
        var expectedTerracedVelocity = (int)InstrumentConfiguration.DefaultMaxVelocity;

        foreach (var note in composition.Measures.Skip(2).Take(2).SelectMany(static measure => measure.Beats).SelectMany(static beat => beat.Chord.Notes))
        {
            ((int)note.Velocity).Should().Be(expectedTerracedVelocity, "75 - 10 clamps to the 60 ceiling exactly once; a second application would land at 50");
        }

        foreach (var note in composition.Measures.Take(2).Concat([composition.Measures[^1]]).SelectMany(static measure => measure.Beats).SelectMany(static beat => beat.Chord.Notes))
        {
            ((int)note.Velocity).Should().Be(75, "the announcement and the close sit outside every terraced statement");
        }
    }

    [Test]
    public void Compose_ClearsTheLedgerBeforeComposing()
    {
        // arrange: a stale entry from a previous composition must not survive into this one
        var staleNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        _voiceRhythmLedger.RecordHeldNote(staleNote);
        _voiceRhythmLedger.RecordDivisionIntensity(staleNote, 140);

        // act
        CreateComposer().Compose(CancellationToken.None);

        // assert
        _voiceRhythmLedger.IsHeldNote(staleNote).Should().BeFalse();
        _voiceRhythmLedger.TryGetDivisionIntensity(staleNote, out _).Should().BeFalse();
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

    private GroundBassComposer CreateComposer() => CreateComposer(relativeComponents: null);

    private GroundBassComposer CreateComposer(GroundBassSectionComponents? relativeComponents) => new(
        _planner,
        new GroundBassSectionComponents(_strategy, _homeSeamStrategy, _selector, _decorator, _tonicizationApplicator),
        relativeComponents,
        _divisionScheduler,
        _voiceRhythmLedger,
        _rule,
        _suspensionApplicator,
        _cadenceClassifier,
        _chordNumberIdentifier,
        _trillApplicator,
        _dynamicsApplicator,
        _fallbackComposer,
        _dispatcher,
        Substitute.For<ILogger>(),
        _configuration
    );

    private GroundBassComposer CreateModulatingComposer()
    {
        _planner.CreatePlan().Returns(_modulatingPlan);

        return CreateComposer(new GroundBassSectionComponents(_relativeStrategy, _relativeSeamStrategy, _relativeSelector, _relativeDecorator, _relativeTonicizationApplicator));
    }

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

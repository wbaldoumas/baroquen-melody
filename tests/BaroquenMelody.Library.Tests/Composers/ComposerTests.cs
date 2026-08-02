using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
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
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Composers;

[TestFixture]
internal sealed class ComposerTests
{
    private static readonly Note MinSopranoNote = Notes.A4;

    private static readonly Note MinAltoNote = Notes.C3;

    private static readonly Note[] SopranoWalk = [Notes.C4, Notes.D4, Notes.E4, Notes.F4, Notes.G4, Notes.A4, Notes.B4, Notes.C5, Notes.D5, Notes.E5, Notes.F5];

    private static readonly Note[] AltoWalk = [Notes.G2, Notes.A2, Notes.B2, Notes.C3, Notes.D3, Notes.E3, Notes.F3, Notes.G3, Notes.A3, Notes.B3, Notes.C4];

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IDynamicsApplicator _mockDynamicsApplicator = null!;

    private ICompositionPhraser _mockCompositionPhraser = null!;

    private ISuspensionApplicator _mockSuspensionApplicator = null!;

    private ITonicizationApplicator _mockTonicizationApplicator = null!;

    private ILogger _mockLogger = null!;

    private IFugalEntryPlacer _fugalEntryPlacer = null!;

    private IChordComposer _chordComposer = null!;

    private IThemeComposer _themeComposer = null!;

    private IEndingComposer _endingComposer = null!;

    private IChordNumberIdentifier _mockChordNumberIdentifier = null!;

    private IDispatcher _mockDispatcher = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private Composer _composer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockCompositionDecorator = Substitute.For<ICompositionDecorator>();
        _mockDynamicsApplicator = Substitute.For<IDynamicsApplicator>();
        _mockCompositionPhraser = Substitute.For<ICompositionPhraser>();
        _mockSuspensionApplicator = Substitute.For<ISuspensionApplicator>();
        _mockTonicizationApplicator = Substitute.For<ITonicizationApplicator>();
        _mockChordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();
        _mockLogger = Substitute.For<ILogger>();
        _mockDispatcher = Substitute.For<IDispatcher>();

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>()).Returns(ChordNumber.V, ChordNumber.I);

        _compositionConfiguration = TestCompositionConfigurations.Get(2);

        var chordSelector = new WeightedChordSelector(new AggregateScoringRule([]), new ThreadLocalRandomProvider());

        _fugalEntryPlacer = new FugalEntryPlacer(_compositionConfiguration);
        _chordComposer = new ChordComposer(_mockCompositionStrategy, chordSelector, _mockLogger);
        _themeComposer = new ThemeComposer(_mockCompositionStrategy, _mockCompositionStrategy, _mockCompositionDecorator, _chordComposer, _fugalEntryPlacer, new FugalAnswerStrategy(_compositionConfiguration), chordSelector, _mockDispatcher, _mockLogger, _compositionConfiguration);
        _endingComposer = new EndingComposer(_mockCompositionStrategy, _mockCompositionDecorator, _mockChordNumberIdentifier, Substitute.For<ICadenceClassifier>(), Substitute.For<ICadentialTrillApplicator>(), chordSelector, _mockDispatcher, _mockLogger, _compositionConfiguration);
        _composer = new Composer(_mockCompositionDecorator, _mockCompositionPhraser, _chordComposer, new HarmonicRhythmScheduler(_compositionConfiguration), new VoiceRhythmScheduler(_compositionConfiguration), new VoiceRhythmLedger(), _mockSuspensionApplicator, _mockTonicizationApplicator, _themeComposer, _endingComposer, _mockDynamicsApplicator, _mockDispatcher, _compositionConfiguration);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        ArrangeComposableStrategy();

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert
        composition.Should().NotBeNull();
        composition.Measures.Should().HaveCountGreaterThanOrEqualTo(_compositionConfiguration.MinimumMeasures);

        foreach (var measure in composition.Measures)
        {
            measure.Beats.Should().HaveCountGreaterThan(0);
        }

        _mockCompositionStrategy.Received(1).GenerateInitialChord();

        // With the default harmonic rhythm, half the body measures (phrase-interior at the default minimum
        // phrase length of two) hold their two weak beats and compose only two chords; the +3 is the theme's
        // initial measure (four beats minus the strategy-generated initial chord).
        var seamMeasureCount = _compositionConfiguration.MinimumMeasures / _compositionConfiguration.PhrasingConfiguration.MinPhraseLength;
        var interiorMeasureCount = _compositionConfiguration.MinimumMeasures - seamMeasureCount;

        _mockCompositionStrategy
            .Received(seamMeasureCount * 4 + interiorMeasureCount * 2 + 3)
            .GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionStrategy
            .Received()
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionDecorator.Received(4).Decorate(Arg.Any<Composition>());
    }

    [Test]
    public void WhenComposeIsInvoked_SuspensionsAreAppliedBeforeTheSustainPass()
    {
        // arrange
        ArrangeComposableStrategy();

        // act
        _composer.Compose(CancellationToken.None);

        // assert - the body's suspensions must see plain default-span material, so they run before
        // sustain merging; the exposition's pass runs at completion, after the body is fully articulated
        Received.InOrder(() =>
        {
            _mockSuspensionApplicator.ApplySuspensions(Arg.Any<Composition>());
            _mockCompositionDecorator.ApplySustain(Arg.Any<Composition>());
            _mockSuspensionApplicator.ApplySuspensions(Arg.Any<Composition>());
        });
    }

    [Test]
    public void WhenComposeIsInvoked_TheThemeExpositionReceivesItsOwnSuspensionPassAtCompletion()
    {
        // arrange
        ArrangeComposableStrategy();

        var suspensionPassCompositions = new List<Composition>();

        _mockSuspensionApplicator.ApplySuspensions(Arg.Do<Composition>(suspensionPassCompositions.Add));

        // act
        _composer.Compose(CancellationToken.None);

        // assert - the exposition is prepended after the body pipeline, so it takes its own pass at
        // completion, with the first body measure riding along untouched as the walk boundary
        suspensionPassCompositions.Should().HaveCount(2);
        suspensionPassCompositions[1].Measures[^1].Should().BeSameAs(suspensionPassCompositions[0].Measures[0]);
    }

    [Test]
    public void WhenComposeIsInvoked_TonicizationRunsBetweenTheSuspensionAndSustainPasses()
    {
        // arrange
        ArrangeComposableStrategy();

        // act
        _composer.Compose(CancellationToken.None);

        // assert - tonicization must see suspension stamps (a raised third may not contradict a figure)
        // and must run before sustain merging so obligation checks read actual sounded notes; the
        // exposition takes its own pass at completion
        Received.InOrder(() =>
        {
            _mockSuspensionApplicator.ApplySuspensions(Arg.Any<Composition>());
            _mockTonicizationApplicator.ApplyTonicization(Arg.Any<Composition>());
            _mockCompositionDecorator.ApplySustain(Arg.Any<Composition>());
            _mockSuspensionApplicator.ApplySuspensions(Arg.Any<Composition>());
            _mockTonicizationApplicator.ApplyTonicization(Arg.Any<Composition>());
        });
    }

    [Test]
    public void WhenComposeIsInvoked_TheThemeExpositionReceivesItsOwnTonicizationPassAtCompletion()
    {
        // arrange
        ArrangeComposableStrategy();

        var tonicizationPassCompositions = new List<Composition>();
        var suspensionPassCompositions = new List<Composition>();

        _mockTonicizationApplicator.ApplyTonicization(Arg.Do<Composition>(tonicizationPassCompositions.Add));
        _mockSuspensionApplicator.ApplySuspensions(Arg.Do<Composition>(suspensionPassCompositions.Add));

        // act
        _composer.Compose(CancellationToken.None);

        // assert - the exposition tonicization pass walks the very same exposition-with-boundary
        // composition its suspension pass took, with the first body measure as the shared boundary
        tonicizationPassCompositions.Should().HaveCount(2);
        tonicizationPassCompositions[1].Should().BeSameAs(suspensionPassCompositions[1]);
        tonicizationPassCompositions[1].Measures[^1].Should().BeSameAs(tonicizationPassCompositions[0].Measures[0]);
    }

    [Test]
    public void WhenHarmonicRhythmIsEnabled_HeldBeatsDuplicateThePrecedingChord()
    {
        // arrange
        ArrangeWalkingComposableStrategy();

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert - held beats are plain duplicates of the preceding chord on the weak beats, left unstamped so
        // the downstream ornamentation and sustain passes can animate or tie them
        var heldBeats = FindHeldBeats(composition, _compositionConfiguration.MinimumMeasures);

        heldBeats.Should().HaveCountGreaterThan(80, because: "half the body measures hold both of their weak beats");
        heldBeats.Should().OnlyContain(static heldBeat => heldBeat.BeatIndex % 2 == 1, because: "holds land only on the weak beats");

        composition.Measures
            .SelectMany(static measure => measure.Beats)
            .SelectMany(static beat => beat.Chord.Notes)
            .Should().OnlyContain(
                static note => note.OrnamentationType != OrnamentationType.Sustain && note.OrnamentationType != OrnamentationType.MidSustain,
                because: "the composer must not pre-stamp sustains onto held beats");
    }

    [Test]
    public void WhenHarmonicRhythmIsDisabled_EveryBeatIsFreshlyComposed()
    {
        // arrange
        ArrangeWalkingComposableStrategy();

        var configurationWithoutHolds = _compositionConfiguration with
        {
            HarmonicRhythmConfiguration = new HarmonicRhythmConfiguration(Enabled: false)
        };

        var composer = new Composer(_mockCompositionDecorator, _mockCompositionPhraser, _chordComposer, new HarmonicRhythmScheduler(configurationWithoutHolds), new VoiceRhythmScheduler(configurationWithoutHolds), new VoiceRhythmLedger(), _mockSuspensionApplicator, _mockTonicizationApplicator, _themeComposer, _endingComposer, _mockDynamicsApplicator, _mockDispatcher, configurationWithoutHolds);

        // act
        var composition = composer.Compose(CancellationToken.None);

        // assert - the walking strategy never repeats a candidate, so any adjacent duplicate beats could only
        // come from the scheduler
        FindHeldBeats(composition, configurationWithoutHolds.MinimumMeasures).Should().BeEmpty();
    }

    private void ArrangeComposableStrategy()
    {
        _mockCompositionStrategy.GenerateInitialChord().Returns(CreateFreshChord());

        _mockCompositionStrategy
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([
                new ChordChoice([
                    new NoteChoice(Instrument.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Instrument.Two, NoteMotion.Oblique, 0)
                ])
            ]);

        // fresh chord instances per call, so held-beat copies can be distinguished from their principals
        _mockCompositionStrategy
            .GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(static _ => [CreateFreshChord()]);

        _mockCompositionStrategy
            .GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(static _ => [CreateFreshChord()]);

        // the fugal entry beats with a successor pin validate their candidates without the free-choice look-ahead
        _mockCompositionStrategy
            .GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(static _ => [CreateFreshChord()]);

        _mockCompositionStrategy
            .HasPossibleChordForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(true);
    }

    /// <summary>
    ///     Arranges the strategy so every candidate chord walks one step further along a diatonic line: consecutive
    ///     candidates always differ from their predecessors, so the only pitch-identical adjacent beats a composition
    ///     can contain are the scheduler's held duplicates.
    /// </summary>
    private void ArrangeWalkingComposableStrategy()
    {
        var callIndex = 0;

        _mockCompositionStrategy.GenerateInitialChord().Returns(_ => CreateWalkingChord(callIndex++));

        _mockCompositionStrategy
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([
                new ChordChoice([
                    new NoteChoice(Instrument.One, NoteMotion.Oblique, 0),
                    new NoteChoice(Instrument.Two, NoteMotion.Oblique, 0)
                ])
            ]);

        _mockCompositionStrategy
            .GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(_ => [CreateWalkingChord(callIndex++)]);

        _mockCompositionStrategy
            .GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(_ => [CreateWalkingChord(callIndex++)]);

        _mockCompositionStrategy
            .GetRuleValidChordsForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(_ => [CreateWalkingChord(callIndex++)]);

        _mockCompositionStrategy
            .HasPossibleChordForPartiallyVoicedChord(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(true);
    }

    /// <summary>
    ///     Finds beats whose chord duplicates the preceding beat's pitches, scanning only the first
    ///     <paramref name="measureLimit"/> measures (the exposition and body) since the ending composer's splice
    ///     and cadence hunt can legitimately restate a chord.
    /// </summary>
    private static List<(int MeasureIndex, int BeatIndex)> FindHeldBeats(Composition composition, int measureLimit)
    {
        var heldBeats = new List<(int MeasureIndex, int BeatIndex)>();

        foreach (var (measureIndex, measure) in composition.Measures.Take(measureLimit).Index())
        {
            for (var beatIndex = 1; beatIndex < measure.Beats.Count; beatIndex++)
            {
                var chord = measure.Beats[beatIndex].Chord;
                var precedingChord = measure.Beats[beatIndex - 1].Chord;

                if (chord.Notes.TrueForAll(note => precedingChord.ContainsInstrument(note.Instrument) && precedingChord[note.Instrument].Raw == note.Raw))
                {
                    heldBeats.Add((measureIndex, beatIndex));
                }
            }
        }

        return heldBeats;
    }

    private static BaroquenChord CreateWalkingChord(int index) => new([
        new BaroquenNote(Instrument.One, SopranoWalk[index % SopranoWalk.Length], MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, AltoWalk[index % AltoWalk.Length], MusicalTimeSpan.Half)
    ]);

    private static BaroquenChord CreateFreshChord() => new([
        new BaroquenNote(Instrument.One, MinSopranoNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, MinAltoNote, MusicalTimeSpan.Half)
    ]);
}

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

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IDynamicsApplicator _mockDynamicsApplicator = null!;

    private ICompositionPhraser _mockCompositionPhraser = null!;

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
        _composer = new Composer(_mockCompositionDecorator, _mockCompositionPhraser, _chordComposer, new HarmonicRhythmScheduler(_compositionConfiguration), _themeComposer, _endingComposer, _mockDynamicsApplicator, _mockDispatcher, _compositionConfiguration);
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
        var accelerationMeasureCount = _compositionConfiguration.MinimumMeasures / _compositionConfiguration.PhrasingConfiguration.MinPhraseLength;
        var interiorMeasureCount = _compositionConfiguration.MinimumMeasures - accelerationMeasureCount;

        _mockCompositionStrategy
            .Received(accelerationMeasureCount * 4 + interiorMeasureCount * 2 + 3)
            .GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionStrategy
            .Received()
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionDecorator.Received(4).Decorate(Arg.Any<Composition>());
    }

    [Test]
    public void WhenHarmonicRhythmIsEnabled_HeldBeatsSustainThePrecedingChord()
    {
        // arrange
        ArrangeComposableStrategy();

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert - held beats are silent mid-sustain copies of the preceding chord, whose notes sustain
        // across both beats
        var heldBeatCount = 0;

        foreach (var measure in composition.Measures)
        {
            for (var beatIndex = 1; beatIndex < measure.Beats.Count; beatIndex++)
            {
                var chord = measure.Beats[beatIndex].Chord;

                if (!chord.Notes.TrueForAll(static note => note.OrnamentationType == OrnamentationType.MidSustain))
                {
                    continue;
                }

                heldBeatCount++;

                var precedingChord = measure.Beats[beatIndex - 1].Chord;

                foreach (var note in chord.Notes)
                {
                    var precedingNote = precedingChord[note.Instrument];

                    precedingNote.Raw.Should().Be(note.Raw);
                    precedingNote.OrnamentationType.Should().Be(OrnamentationType.Sustain);
                    precedingNote.MusicalTimeSpan.Should().Be(_compositionConfiguration.DefaultNoteTimeSpan + _compositionConfiguration.DefaultNoteTimeSpan);
                }
            }
        }

        heldBeatCount.Should().BeGreaterThan(0);
    }

    [Test]
    public void WhenHarmonicRhythmIsDisabled_EveryBeatIsFreshlyComposed()
    {
        // arrange
        ArrangeComposableStrategy();

        var configurationWithoutHolds = _compositionConfiguration with
        {
            HarmonicRhythmConfiguration = new HarmonicRhythmConfiguration(Enabled: false)
        };

        var composer = new Composer(_mockCompositionDecorator, _mockCompositionPhraser, _chordComposer, new HarmonicRhythmScheduler(configurationWithoutHolds), _themeComposer, _endingComposer, _mockDynamicsApplicator, _mockDispatcher, configurationWithoutHolds);

        // act
        var composition = composer.Compose(CancellationToken.None);

        // assert - no beat carries scheduler-injected sustains
        composition.Measures
            .SelectMany(static measure => measure.Beats)
            .SelectMany(static beat => beat.Chord.Notes)
            .Should().OnlyContain(static note => note.OrnamentationType != OrnamentationType.Sustain && note.OrnamentationType != OrnamentationType.MidSustain);
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

    private static BaroquenChord CreateFreshChord() => new([
        new BaroquenNote(Instrument.One, MinSopranoNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, MinAltoNote, MusicalTimeSpan.Half)
    ]);
}

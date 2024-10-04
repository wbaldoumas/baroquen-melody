using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Phrasing;
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

    private INoteTransposer _noteTransposer = null!;

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

        _noteTransposer = new NoteTransposer(_compositionConfiguration);
        _chordComposer = new ChordComposer(_mockCompositionStrategy, _mockLogger);
        _themeComposer = new ThemeComposer(_mockCompositionStrategy, _mockCompositionDecorator, _chordComposer, _noteTransposer, _mockDispatcher, _mockLogger, _compositionConfiguration);
        _endingComposer = new EndingComposer(_mockCompositionStrategy, _mockCompositionDecorator, _mockChordNumberIdentifier, _mockDispatcher, _mockLogger, _compositionConfiguration);
        _composer = new Composer(_mockCompositionDecorator, _mockCompositionPhraser, _chordComposer, _themeComposer, _endingComposer, _mockDynamicsApplicator, _mockDispatcher, _compositionConfiguration);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        _mockCompositionStrategy.GenerateInitialChord().Returns(
            new BaroquenChord([
                new BaroquenNote(Instrument.One, MinSopranoNote, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, MinAltoNote, MusicalTimeSpan.Half)
            ])
        );

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
            .Returns([
                new BaroquenChord([
                    new BaroquenNote(Instrument.One, MinSopranoNote, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, MinAltoNote, MusicalTimeSpan.Half)
                ])
            ]);

        _mockCompositionStrategy
            .GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([
                new BaroquenChord([
                    new BaroquenNote(Instrument.One, MinSopranoNote, MusicalTimeSpan.Half),
                    new BaroquenNote(Instrument.Two, MinAltoNote, MusicalTimeSpan.Half)
                ])
            ]);

        // act
        var composition = _composer.Compose(CancellationToken.None);

        // assert
        composition.Should().NotBeNull();
        composition.Measures.Should().HaveCountGreaterOrEqualTo(_compositionConfiguration.MinimumMeasures);

        foreach (var measure in composition.Measures)
        {
            measure.Beats.Should().HaveCountGreaterThan(0);
        }

        _mockCompositionStrategy.Received(1).GenerateInitialChord();

        _mockCompositionStrategy
            .Received(_compositionConfiguration.MinimumMeasures * 4 + 3) // minimum measures * beats per measure + initial measure - initial chord
            .GetPossibleChords(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionStrategy
            .Received()
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionDecorator.Received(4).Decorate(Arg.Any<Composition>());
    }
}

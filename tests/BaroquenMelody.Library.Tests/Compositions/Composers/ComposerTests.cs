using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Composers;

[TestFixture]
internal sealed class ComposerTests
{
    private static readonly Note MinSopranoNote = Notes.A4;
    private static readonly Note MaxSopranoNote = Notes.A5;

    private static readonly Note MinAltoNote = Notes.C3;
    private static readonly Note MaxAltoNote = Notes.C4;

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private ICompositionPhraser _mockCompositionPhraser = null!;

    private INoteTransposer _noteTransposer = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private Composer _composer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockCompositionDecorator = Substitute.For<ICompositionDecorator>();
        _mockCompositionPhraser = Substitute.For<ICompositionPhraser>();

        _compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, MinSopranoNote, MaxSopranoNote),
                new(Voice.Alto, MinAltoNote, MaxAltoNote)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _noteTransposer = new NoteTransposer(_compositionConfiguration);

        _composer = new Composer(_mockCompositionStrategy, _mockCompositionDecorator, _mockCompositionPhraser, _noteTransposer, _compositionConfiguration);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        _mockCompositionStrategy.GenerateInitialChord().Returns(
            new BaroquenChord([
                new BaroquenNote(Voice.Soprano, MinSopranoNote),
                new BaroquenNote(Voice.Alto, MinAltoNote)
            ])
        );

        _mockCompositionStrategy
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([
                new ChordChoice([
                    new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                ])
            ]);

        _mockCompositionStrategy
            .GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([
                new BaroquenChord([
                    new BaroquenNote(Voice.Soprano, MinSopranoNote),
                    new BaroquenNote(Voice.Alto, MinAltoNote)
                ])
            ]);

        // act
        var composition = _composer.Compose();

        // assert
        composition.Should().NotBeNull();
        composition.Measures.Should().HaveCountGreaterOrEqualTo(_compositionConfiguration.CompositionLength);

        foreach (var measure in composition.Measures)
        {
            measure.Beats.Should().HaveCount(_compositionConfiguration.Meter.BeatsPerMeasure());
        }

        _mockCompositionStrategy.Received(1).GenerateInitialChord();

        _mockCompositionStrategy
            .Received(_compositionConfiguration.CompositionLength * _compositionConfiguration.Meter.BeatsPerMeasure() + 3) // 3 more to account for fugue subjects
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionDecorator.Received(2).Decorate(Arg.Any<Composition>());
    }
}

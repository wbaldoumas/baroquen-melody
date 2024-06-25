using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Tests.Compositions.Enums.Extensions;
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

        _composer = new Composer(_mockCompositionStrategy, _mockCompositionDecorator, _mockCompositionPhraser, _compositionConfiguration);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        _mockCompositionStrategy.GenerateInitialChord().Returns(
            new BaroquenChord(
                [
                    new BaroquenNote(Voice.Soprano, MinSopranoNote),
                    new BaroquenNote(Voice.Alto, MinAltoNote)
                ]
            )
        );

        _mockCompositionStrategy
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(
                new List<ChordChoice>
                {
                    new(
                        [
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ]
                    )
                }
            );

        // act
        var composition = _composer.Compose();

        // assert
        composition.Should().NotBeNull();
        composition.Measures.Should().HaveCount(_compositionConfiguration.CompositionLength);

        foreach (var measure in composition.Measures)
        {
            measure.Beats.Should().HaveCount(_compositionConfiguration.Meter.BeatsPerMeasure());
        }

        _mockCompositionStrategy.Received(1).GenerateInitialChord();

        _mockCompositionStrategy
            .Received(_compositionConfiguration.CompositionLength * _compositionConfiguration.Meter.BeatsPerMeasure() - 1) // 1 less since the initial chord is generated separately
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>());

        _mockCompositionDecorator.Received(1).Decorate(composition);
    }
}

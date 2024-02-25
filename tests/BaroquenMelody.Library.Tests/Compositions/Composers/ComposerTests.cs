using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Tests.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Composers;

[TestFixture]
internal sealed class ComposerTests
{
    private static readonly Note MinSopranoNote = Note.Get(NoteName.A, 4);
    private static readonly Note MaxSopranoNote = Note.Get(NoteName.A, 5);

    private static readonly Note MinAltoNote = Note.Get(NoteName.C, 3);
    private static readonly Note MaxAltoNote = Note.Get(NoteName.C, 4);

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private IChordContextGenerator _mockChordContextGenerator = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private Composer _composer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockChordContextGenerator = Substitute.For<IChordContextGenerator>();

        _compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, MinSopranoNote, MaxSopranoNote),
                new(Voice.Alto, MinAltoNote, MaxAltoNote)
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _composer = new Composer(_mockCompositionStrategy, _mockChordContextGenerator, _compositionConfiguration);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        _mockCompositionStrategy.GetInitialChord().Returns(
            new ContextualizedChord(
                new HashSet<ContextualizedNote>
                {
                    new(
                        MinSopranoNote,
                        Voice.Soprano,
                        new NoteContext(Voice.Soprano, MinSopranoNote, NoteMotion.Oblique, NoteSpan.None),
                        new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                    ),
                    new(
                        MinAltoNote,
                        Voice.Alto,
                        new NoteContext(Voice.Alto, MinAltoNote, NoteMotion.Oblique, NoteSpan.None),
                        new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                    )
                },
                new ChordContext(
                [
                    new NoteContext(Voice.Soprano, MinSopranoNote, NoteMotion.Oblique, NoteSpan.None),
                    new NoteContext(Voice.Alto, MinAltoNote, NoteMotion.Oblique, NoteSpan.None)
                ]),
                new ChordChoice(
                [
                    new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
                    new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                ])
            )
        );

        _mockCompositionStrategy.GetNextChordChoice(Arg.Any<ChordContext>())
            .Returns(
                new ChordChoice(
                    [
                        new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
                        new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                    ]
                )
            );

        _mockChordContextGenerator.GenerateChordContext(Arg.Any<ContextualizedChord>(), Arg.Any<ContextualizedChord>())
            .Returns(
                new ChordContext(
                [
                    new NoteContext(Voice.Soprano, MinSopranoNote, NoteMotion.Oblique, NoteSpan.None),
                    new NoteContext(Voice.Alto, MinAltoNote, NoteMotion.Oblique, NoteSpan.None)
                ])
            );

        // act
        var composition = _composer.Compose();

        // assert
        composition.Should().NotBeNull();
        composition.Measures.Should().HaveCount(_compositionConfiguration.CompositionLength);

        _mockCompositionStrategy.Received(1).GetInitialChord();

        _mockCompositionStrategy
            .Received(_compositionConfiguration.CompositionLength * _compositionConfiguration.Meter.BeatsPerMeasure())
            .GetNextChordChoice(Arg.Any<ChordContext>());
    }
}

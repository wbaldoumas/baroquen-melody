using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Tests.Compositions.Enums.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using Chord = BaroquenMelody.Library.Compositions.Domain.Chord;
using Note = BaroquenMelody.Library.Compositions.Domain.Note;

namespace BaroquenMelody.Library.Tests.Compositions.Composers;

[TestFixture]
internal sealed class ComposerTests
{
    private static readonly Melanchall.DryWetMidi.MusicTheory.Note MinSopranoNote = Melanchall.DryWetMidi.MusicTheory.Note.Get(NoteName.A, 4);
    private static readonly Melanchall.DryWetMidi.MusicTheory.Note MaxSopranoNote = Melanchall.DryWetMidi.MusicTheory.Note.Get(NoteName.A, 5);

    private static readonly Melanchall.DryWetMidi.MusicTheory.Note MinAltoNote = Melanchall.DryWetMidi.MusicTheory.Note.Get(NoteName.C, 3);
    private static readonly Melanchall.DryWetMidi.MusicTheory.Note MaxAltoNote = Melanchall.DryWetMidi.MusicTheory.Note.Get(NoteName.C, 4);

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private Composer _composer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();

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

        _composer = new Composer(_mockCompositionStrategy, _compositionConfiguration);
    }

    [Test]
    public void WhenComposeIsInvoked_ThenCompositionIsReturned()
    {
        // arrange
        _mockCompositionStrategy.GenerateInitialChord().Returns(
            new Chord(
                [
                    new Note(Voice.Soprano, MinSopranoNote),
                    new Note(Voice.Alto, MinAltoNote)
                ]
            )
        );

        _mockCompositionStrategy
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<Chord>>())
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
            .GetPossibleChordChoices(Arg.Any<IReadOnlyList<Chord>>());
    }
}

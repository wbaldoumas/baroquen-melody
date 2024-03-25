using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;
using Chord = BaroquenMelody.Library.Compositions.Domain.Chord;
using Note = BaroquenMelody.Library.Compositions.Domain.Note;

namespace BaroquenMelody.Library.Tests.Compositions.Strategies;

[TestFixture]
internal sealed class CompositionStrategyTests
{
    private const byte MinSopranoPitch = 60;

    private const byte MaxSopranoPitch = 72;

    private const byte MinAltoPitch = 48;

    private const byte MaxAltoPitch = 60;

    private const byte MinTenorPitch = 36;

    private const byte MaxTenorPitch = 48;

    private const byte MinBassPitch = 24;

    private const byte MaxBassPitch = 36;

    private static readonly BigInteger MockChordChoiceCount = 5;

    private IChordChoiceRepository _mockChordChoiceRepository = default!;

    private ICompositionRule _mockCompositionRule = default!;

    private CompositionStrategy _compositionStrategy = default!;

    private CompositionConfiguration _compositionConfiguration = default!;

    [SetUp]
    public void Setup()
    {
        _mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();

        _mockChordChoiceRepository.Count.Returns(MockChordChoiceCount);

        _compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, MinSopranoPitch.ToNote(), MaxSopranoPitch.ToNote()),
                new(Voice.Alto, MinAltoPitch.ToNote(), MaxAltoPitch.ToNote()),
                new(Voice.Tenor, MinTenorPitch.ToNote(), MaxTenorPitch.ToNote()),
                new(Voice.Bass, MinBassPitch.ToNote(), MaxBassPitch.ToNote())
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _compositionStrategy = new CompositionStrategy(
            _mockChordChoiceRepository,
            _mockCompositionRule,
            _compositionConfiguration
        );
    }

    [Test]
    public void GetInitialChord_generates_valid_chord()
    {
        // run this test a bunch to account for randomness
        for (var i = 0; i < 1000; ++i)
        {
            // act
            var chord = _compositionStrategy.GenerateInitialChord();

            // assert
            chord.Notes.Count().Should().Be(4);

            // since there are four voices but only three unique notes in a C Major chord, there should be at least one voice with a repeated note
            chord.Notes.GroupBy(note => note.Raw.NoteName).Should().HaveCount(3).And.OnlyContain(grouping => grouping.Count() <= 2);

            foreach (var contextualizedNote in chord.Notes)
            {
                contextualizedNote.Raw.NoteName.Should().BeOneOf(
                    NoteName.C,
                    NoteName.E,
                    NoteName.G
                );
            }
        }
    }

    [Test]
    public void GetPossibleChordChoices_returns_expected_chord_choices()
    {
        // arrange
        var precedingChords = new List<Chord>
        {
            new([
                new Note(Voice.Soprano, Notes.C4),
                new Note(Voice.Alto, Notes.E3),
                new Note(Voice.Tenor, Notes.G2),
                new Note(Voice.Bass, Notes.C2)
            ])
        };

        var goodChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
        ]);

        var badChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 5),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 5)
        ]);

        _mockChordChoiceRepository.GetChordChoice(Arg.Any<BigInteger>()).Returns(goodChordChoice, badChordChoice);

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<Chord>>(), Arg.Any<Chord>()).Returns(true, false);

        // act
        var possibleChordChoices = _compositionStrategy.GetPossibleChordChoices(precedingChords).ToList();

        // Assert
        possibleChordChoices
            .Should()
            .ContainSingle(chordChoice => chordChoice.Equals(goodChordChoice), "because the chord choice passed the composition rule");
    }
}

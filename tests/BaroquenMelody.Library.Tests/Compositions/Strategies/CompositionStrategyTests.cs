using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Random;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Numerics;

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

    private static readonly BigInteger MockChordContextCount = 5;

    private static readonly BigInteger MockChordChoiceCount = 5;

    private static readonly bool[] AllTrue = [true, true, true, true, true];

    private static readonly bool[] Alternating = [true, false, true, false, true];

    private IChordChoiceRepository _mockChordChoiceRepository = default!;

    private IChordContextRepository _mockChordContextRepository = default!;

    private IRandomTrueIndexSelector _mockRandomTrueIndexSelector = default!;

    private IDictionary<BigInteger, BitArray> _mockChordContextToChordChoiceMap = default!;

    private CompositionStrategy _compositionStrategy = default!;

    private CompositionConfiguration _compositionConfiguration = default!;

    [SetUp]
    public void Setup()
    {
        _mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();
        _mockChordContextRepository = Substitute.For<IChordContextRepository>();
        _mockRandomTrueIndexSelector = Substitute.For<IRandomTrueIndexSelector>();
        _mockChordContextToChordChoiceMap = Substitute.For<IDictionary<BigInteger, BitArray>>();

        _mockChordChoiceRepository.Count.Returns(MockChordChoiceCount);
        _mockChordContextRepository.Count.Returns(MockChordContextCount);

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
            _mockChordContextRepository,
            _mockRandomTrueIndexSelector,
            _mockChordContextToChordChoiceMap,
            _compositionConfiguration
        );
    }

    [Test]
    public void GetNextChordChoice_returns_random_chord_choice()
    {
        // arrange
        var chordContext = new ChordContext(
        [
            new NoteContext(Voice.Soprano, (MaxSopranoPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Alto, (MinAltoPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Tenor, (MaxTenorPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Bass, (MinBassPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step)
        ]);

        var chordChoice = new ChordChoice(
        [
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Alto, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Bass, NoteMotion.Ascending, 1)
        ]);

        const int chordContextId = 3;
        const int chordChoiceId = 3;

        var bitArray = new BitArray(Alternating);

        _mockChordContextRepository.GetChordContextId(chordContext).Returns(chordContextId);
        _mockChordContextToChordChoiceMap[chordContextId].Returns(bitArray);
        _mockRandomTrueIndexSelector.SelectRandomTrueIndex(bitArray).Returns(chordChoiceId);
        _mockChordChoiceRepository.GetChordChoice(chordChoiceId).Returns(chordChoice);

        // act
        var result = _compositionStrategy.GetNextChordChoice(chordContext);

        // assert
        result.Should().BeEquivalentTo(chordChoice);

        Received.InOrder(
            () =>
            {
                _mockChordContextRepository.GetChordContextId(chordContext);
                _mockRandomTrueIndexSelector.SelectRandomTrueIndex(Arg.Any<BitArray>());
                _mockChordChoiceRepository.GetChordChoice(chordChoiceId);
            }
        );
    }

    [Test]
    public void GetNextChordChoice_invalidates_choices_out_of_voice_range_and_returns_random_chord_choice()
    {
        // arrange
        var chordContext = new ChordContext(
        [
            new NoteContext(Voice.Soprano, (MaxSopranoPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Alto, (MinAltoPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Tenor, (MaxTenorPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Bass, (MinBassPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step)
        ]);

        var invalidChordChoiceA = new ChordChoice(
        [
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 5),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 5)
        ]);

        var invalidChordChoiceB = new ChordChoice(
        [
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 6),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 6),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 6),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 6)
        ]);

        var validChordChoice = new ChordChoice(
        [
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Alto, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Bass, NoteMotion.Ascending, 1)
        ]);

        const int chordContextId = 3;
        const int invalidChordChoiceIdA = 2;
        const int invalidChordChoiceIdB = 3;
        const int validChordChoiceId = 4;

        var bitArray = new BitArray(AllTrue);

        _mockChordContextRepository.GetChordContextId(chordContext).Returns(chordContextId);
        _mockChordContextToChordChoiceMap[chordContextId].Returns(bitArray);

        _mockRandomTrueIndexSelector.SelectRandomTrueIndex(bitArray).Returns(
            invalidChordChoiceIdA,
            invalidChordChoiceIdB,
            validChordChoiceId
        );

        _mockChordChoiceRepository.GetChordChoice(invalidChordChoiceIdA).Returns(invalidChordChoiceA);
        _mockChordChoiceRepository.GetChordChoice(invalidChordChoiceIdB).Returns(invalidChordChoiceB);
        _mockChordChoiceRepository.GetChordChoice(validChordChoiceId).Returns(validChordChoice);

        _mockChordChoiceRepository.GetChordChoiceId(invalidChordChoiceA).Returns(invalidChordChoiceIdA);
        _mockChordChoiceRepository.GetChordChoiceId(invalidChordChoiceB).Returns(invalidChordChoiceIdB);

        // act
        var result = _compositionStrategy.GetNextChordChoice(chordContext);

        // assert
        result.Should().BeEquivalentTo(validChordChoice);

        bitArray[invalidChordChoiceIdA].Should().BeFalse();
        bitArray[invalidChordChoiceIdB].Should().BeFalse();
        bitArray[validChordChoiceId].Should().BeTrue();
    }

    [Test]
    public void InvalidateChordChoice_sets_bit_to_false()
    {
        // arrange
        var chordContext = new ChordContext(
        [
            new NoteContext(Voice.Soprano, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Alto, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Tenor, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
            new NoteContext(Voice.Bass, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step)
        ]);

        var chordChoice = new ChordChoice(
        [
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Alto, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Bass, NoteMotion.Ascending, 5)
        ]);

        const int chordContextId = 3;
        const int chordChoiceId = 3;

        var bitArray = new BitArray(5, true);

        _mockChordContextRepository.GetChordContextId(chordContext).Returns(chordContextId);
        _mockChordChoiceRepository.GetChordChoiceId(chordChoice).Returns(chordChoiceId);
        _mockChordContextToChordChoiceMap[chordContextId].Returns(bitArray);

        // act
        _compositionStrategy.InvalidateChordChoice(chordContext, chordChoice);

        // assert
        bitArray[chordChoiceId].Should().BeFalse();

        Received.InOrder(
            () =>
            {
                _mockChordContextRepository.GetChordContextId(chordContext);
                _mockChordChoiceRepository.GetChordChoiceId(chordChoice);
            }
        );
    }

    [Test]
    public void GetInitialChord_generates_valid_chord()
    {
        // run this test a bunch to account for randomness
        for (var i = 0; i < 1000; ++i)
        {
            // act
            var chord = _compositionStrategy.GetInitialChord();

            // assert
            chord.Notes.Count.Should().Be(4);

            // since there are four voices but only three unique notes in a C Major chord, there should be at least one voice with a repeated note
            chord.Notes.GroupBy(note => note.Note.NoteName).Should().HaveCount(3).And.OnlyContain(grouping => grouping.Count() <= 2);

            foreach (var contextualizedNote in chord.Notes)
            {
                contextualizedNote.Note.NoteName.Should().BeOneOf(
                    NoteName.C,
                    NoteName.E,
                    NoteName.G
                );
            }
        }
    }
}

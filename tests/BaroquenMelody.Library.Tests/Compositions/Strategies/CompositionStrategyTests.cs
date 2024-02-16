using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Random;
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
            Scale.Parse("C Major")
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
            new List<NoteContext>
            {
                new(Voice.Soprano, (MaxSopranoPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Alto, (MinAltoPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Tenor, (MaxTenorPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Bass, (MinBassPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step)
            }
        );

        var chordChoice = new ChordChoice(
            new List<NoteChoice>
            {
                new(Voice.Soprano, NoteMotion.Ascending, 1),
                new(Voice.Alto, NoteMotion.Ascending, 1),
                new(Voice.Tenor, NoteMotion.Ascending, 1),
                new(Voice.Bass, NoteMotion.Ascending, 1)
            }
        );

        const int chordContextIndex = 3;
        const int chordChoiceIndex = 3;

        var bitArray = new BitArray(new[] { true, false, false, true, false });

        _mockChordContextRepository.GetChordContextIndex(chordContext).Returns(chordContextIndex);
        _mockChordContextToChordChoiceMap[chordContextIndex].Returns(bitArray);
        _mockRandomTrueIndexSelector.SelectRandomTrueIndex(bitArray).Returns(chordChoiceIndex);
        _mockChordChoiceRepository.GetChordChoice(chordChoiceIndex).Returns(chordChoice);

        // act
        var result = _compositionStrategy.GetNextChordChoice(chordContext);

        // assert
        result.Should().BeEquivalentTo(chordChoice);

        Received.InOrder(
            () =>
            {
                _mockChordContextRepository.GetChordContextIndex(chordContext);
                _mockRandomTrueIndexSelector.SelectRandomTrueIndex(Arg.Any<BitArray>());
                _mockChordChoiceRepository.GetChordChoice(chordChoiceIndex);
            }
        );
    }

    [Test]
    public void GetNextChordChoice_invalidates_choices_out_of_voice_range_and_returns_random_chord_choice()
    {
        // arrange
        var chordContext = new ChordContext(
            new List<NoteContext>
            {
                new(Voice.Soprano, (MaxSopranoPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Alto, (MinAltoPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Tenor, (MaxTenorPitch - 1).ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Bass, (MinBassPitch + 2).ToNote(), NoteMotion.Ascending, NoteSpan.Step)
            }
        );

        var invalidChordChoiceA = new ChordChoice(
            new List<NoteChoice>
            {
                new(Voice.Soprano, NoteMotion.Ascending, 5),
                new(Voice.Alto, NoteMotion.Descending, 5),
                new(Voice.Tenor, NoteMotion.Ascending, 5),
                new(Voice.Bass, NoteMotion.Descending, 5)
            }
        );

        var invalidChordChoiceB = new ChordChoice(
            new List<NoteChoice>
            {
                new(Voice.Soprano, NoteMotion.Ascending, 6),
                new(Voice.Alto, NoteMotion.Descending, 6),
                new(Voice.Tenor, NoteMotion.Ascending, 6),
                new(Voice.Bass, NoteMotion.Descending, 6)
            }
        );

        var validChordChoice = new ChordChoice(
            new List<NoteChoice>
            {
                new(Voice.Soprano, NoteMotion.Ascending, 1),
                new(Voice.Alto, NoteMotion.Ascending, 1),
                new(Voice.Tenor, NoteMotion.Ascending, 1),
                new(Voice.Bass, NoteMotion.Ascending, 1)
            }
        );

        const int chordContextIndex = 3;
        const int invalidChordChoiceIndexA = 2;
        const int invalidChordChoiceIndexB = 3;
        const int validChordChoiceIndex = 4;

        var bitArray = new BitArray(new[] { true, true, true, true, true });

        _mockChordContextRepository.GetChordContextIndex(chordContext).Returns(chordContextIndex);
        _mockChordContextToChordChoiceMap[chordContextIndex].Returns(bitArray);

        _mockRandomTrueIndexSelector.SelectRandomTrueIndex(bitArray).Returns(
            invalidChordChoiceIndexA,
            invalidChordChoiceIndexB,
            validChordChoiceIndex
        );

        _mockChordChoiceRepository.GetChordChoice(invalidChordChoiceIndexA).Returns(invalidChordChoiceA);
        _mockChordChoiceRepository.GetChordChoice(invalidChordChoiceIndexB).Returns(invalidChordChoiceB);
        _mockChordChoiceRepository.GetChordChoice(validChordChoiceIndex).Returns(validChordChoice);

        _mockChordChoiceRepository.GetChordChoiceIndex(invalidChordChoiceA).Returns(invalidChordChoiceIndexA);
        _mockChordChoiceRepository.GetChordChoiceIndex(invalidChordChoiceB).Returns(invalidChordChoiceIndexB);

        // act
        var result = _compositionStrategy.GetNextChordChoice(chordContext);

        // assert
        result.Should().BeEquivalentTo(validChordChoice);

        bitArray[invalidChordChoiceIndexA].Should().BeFalse();
        bitArray[invalidChordChoiceIndexB].Should().BeFalse();
        bitArray[validChordChoiceIndex].Should().BeTrue();
    }

    [Test]
    public void InvalidateChordChoice_sets_bit_to_false()
    {
        // arrange
        var chordContext = new ChordContext(
            new List<NoteContext>
            {
                new(Voice.Soprano, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Alto, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Tenor, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Bass, 25.ToNote(), NoteMotion.Ascending, NoteSpan.Step)
            }
        );

        var chordChoice = new ChordChoice(
            new List<NoteChoice>
            {
                new(Voice.Soprano, NoteMotion.Ascending, 5),
                new(Voice.Alto, NoteMotion.Ascending, 5),
                new(Voice.Tenor, NoteMotion.Ascending, 5),
                new(Voice.Bass, NoteMotion.Ascending, 5)
            }
        );

        const int chordContextIndex = 3;
        const int chordChoiceIndex = 3;

        var bitArray = new BitArray(5, true);

        _mockChordContextRepository.GetChordContextIndex(chordContext).Returns(chordContextIndex);
        _mockChordChoiceRepository.GetChordChoiceIndex(chordChoice).Returns(chordChoiceIndex);
        _mockChordContextToChordChoiceMap[chordContextIndex].Returns(bitArray);

        // act
        _compositionStrategy.InvalidateChordChoice(chordContext, chordChoice);

        // assert
        bitArray[chordChoiceIndex].Should().BeFalse();

        Received.InOrder(
            () =>
            {
                _mockChordContextRepository.GetChordContextIndex(chordContext);
                _mockChordChoiceRepository.GetChordChoiceIndex(chordChoice);
            }
        );
    }
}

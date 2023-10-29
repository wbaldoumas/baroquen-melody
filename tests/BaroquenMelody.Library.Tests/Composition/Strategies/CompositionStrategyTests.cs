using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using BaroquenMelody.Library.Composition.Strategies;
using BaroquenMelody.Library.Random;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Tests.Composition.Strategies;

[TestFixture]
internal sealed class CompositionStrategyTests
{
    private static readonly BigInteger MockChordContextCount = 5;

    private static readonly BigInteger MockChordChoiceCount = 5;

    private IChordChoiceRepository _mockChordChoiceRepository = default!;

    private IChordContextRepository _mockChordContextRepository = default!;

    private IRandomTrueIndexSelector _mockRandomTrueIndexSelector = default!;

    private IDictionary<BigInteger, BitArray> _mockChordContextToChordChoiceMap = default!;

    private CompositionStrategy _compositionStrategy = default!;

    [SetUp]
    public void Setup()
    {
        _mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();
        _mockChordContextRepository = Substitute.For<IChordContextRepository>();
        _mockRandomTrueIndexSelector = Substitute.For<IRandomTrueIndexSelector>();
        _mockChordContextToChordChoiceMap = Substitute.For<IDictionary<BigInteger, BitArray>>();

        _mockChordChoiceRepository.Count.Returns(MockChordChoiceCount);
        _mockChordContextRepository.Count.Returns(MockChordContextCount);

        _compositionStrategy = new CompositionStrategy(
            _mockChordChoiceRepository,
            _mockChordContextRepository,
            _mockRandomTrueIndexSelector,
            _mockChordContextToChordChoiceMap
        );
    }

    [Test]
    public void GetNextChordChoice_returns_random_chord_choice()
    {
        // arrange
        var chordContext = new ChordContext(
            new List<NoteContext>
            {
                new(Voice.Soprano, 25, NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Alto, 25, NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Tenor, 25, NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Bass, 25, NoteMotion.Ascending, NoteSpan.Step)
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
    public void InvalidateChordChoice_sets_bit_to_false()
    {
        // arrange
        var chordContext = new ChordContext(
            new List<NoteContext>
            {
                new(Voice.Soprano, 25, NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Alto, 25, NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Tenor, 25, NoteMotion.Ascending, NoteSpan.Step),
                new(Voice.Bass, 25, NoteMotion.Ascending, NoteSpan.Step)
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

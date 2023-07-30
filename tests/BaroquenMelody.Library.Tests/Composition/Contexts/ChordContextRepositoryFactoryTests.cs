using BaroquenMelody.Library.Composition.Configurations;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Contexts;

[TestFixture]
internal sealed class ChordContextRepositoryFactoryTests
{
    private INoteContextGenerator _mockNoteContextGenerator = null!;
    private IChordContextRepositoryFactory _chordContextRepositoryFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNoteContextGenerator = Substitute.For<INoteContextGenerator>();
        _chordContextRepositoryFactory = new ChordContextRepositoryFactory(_mockNoteContextGenerator);
    }

    [Test]
    [TestCase(2, typeof(DuetChordContextRepository))]
    [TestCase(3, typeof(TrioChordContextRepository))]
    [TestCase(4, typeof(QuartetChordContextRepository))]
    public void WhenChordContextRepositoryFactoryCreatesChordContextRepository_ItReturnsExpectedType(
        int numberOfVoices,
        Type expectedType)
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            Enumerable.Range(0, numberOfVoices)
                .Select(index => new VoiceConfiguration((Voice)index, (byte)index, (byte)(index + 1)))
                .ToHashSet()
        );

        // act
        var chordContextRepository = _chordContextRepositoryFactory.Create(compositionConfiguration);

        // assert
        chordContextRepository.Should().BeOfType(expectedType);
    }

    [Test]
    public void WhenChordContextRepositoryIsPassedInvalidConfiguration_ItThrows()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                // invalid configuration: only one voice
                new(Voice.Soprano, 55, 90)
            }
        );

        // act
        var act = () => _chordContextRepositoryFactory.Create(compositionConfiguration);

        // assert
        act.Should().Throw<ArgumentException>();
    }
}

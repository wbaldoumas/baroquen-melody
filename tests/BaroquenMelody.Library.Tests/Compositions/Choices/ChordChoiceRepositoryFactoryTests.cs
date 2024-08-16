using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Choices;

[TestFixture]
internal sealed class ChordChoiceRepositoryFactoryTests
{
    private INoteChoiceGenerator _mockNoteChoiceGenerator = null!;

    private ChordChoiceRepositoryFactory _chordChoiceRepositoryFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNoteChoiceGenerator = Substitute.For<INoteChoiceGenerator>();
        _chordChoiceRepositoryFactory = new ChordChoiceRepositoryFactory(_mockNoteChoiceGenerator);
    }

    [Test]
    [TestCase(2, typeof(DuetChordChoiceRepository))]
    [TestCase(3, typeof(TrioChordChoiceRepository))]
    [TestCase(4, typeof(QuartetChordChoiceRepository))]
    public void WhenChordChoiceRepositoryFactoryCreatesChordChoiceRepository_ItReturnsExpectedType(
        int numberOfInstruments,
        Type expectedType)
    {
        // act
        var chordChoiceRepository = _chordChoiceRepositoryFactory.Create(Configurations.GetCompositionConfiguration(numberOfInstruments));

        // assert
        chordChoiceRepository.Should().BeOfType(expectedType);
    }

    [Test]
    public void WhenChordChoiceRepositoryIsPassedInvalidConfiguration_ItThrows()
    {
        // act
        var act = () => _chordChoiceRepositoryFactory.Create(Configurations.GetCompositionConfiguration(1));

        // assert
        act.Should().Throw<ArgumentException>();
    }
}

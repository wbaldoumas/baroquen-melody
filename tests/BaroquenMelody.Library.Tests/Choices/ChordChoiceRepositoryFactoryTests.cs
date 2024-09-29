using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Choices;

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
    [TestCase(1, typeof(SoloChordChoiceRepository))]
    [TestCase(2, typeof(DuetChordChoiceRepository))]
    [TestCase(3, typeof(TrioChordChoiceRepository))]
    [TestCase(4, typeof(QuartetChordChoiceRepository))]
    public void WhenChordChoiceRepositoryFactoryCreatesChordChoiceRepository_ItReturnsExpectedType(
        int numberOfInstruments,
        Type expectedType)
    {
        // act
        var chordChoiceRepository = _chordChoiceRepositoryFactory.Create(TestCompositionConfigurations.GetCompositionConfiguration(numberOfInstruments));

        // assert
        chordChoiceRepository.Should().BeOfType(expectedType);
    }

    [Test]
    public void WhenChordChoiceRepositoryIsPassedInvalidConfiguration_ItThrows()
    {
        // act
        var act = () => _chordChoiceRepositoryFactory.Create(TestCompositionConfigurations.GetCompositionConfiguration(0));

        // assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("The composition configuration must contain between one and four instrument configurations. (Parameter 'compositionConfiguration')");
    }
}

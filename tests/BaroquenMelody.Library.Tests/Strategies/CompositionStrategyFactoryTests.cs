using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Strategies;

[TestFixture]
internal sealed class CompositionStrategyFactoryTests
{
    private IChordChoiceRepositoryFactory _mockChordChoiceRepositoryFactory = null!;

    private ICompositionRule _mockCompositionRule = null!;

    private ILogger _mockLogger = null!;

    private CompositionStrategyFactory _compositionStrategyFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockChordChoiceRepositoryFactory = Substitute.For<IChordChoiceRepositoryFactory>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionStrategyFactory = new CompositionStrategyFactory(_mockChordChoiceRepositoryFactory, _mockCompositionRule, _mockLogger);
    }

    [Test]
    public void CreateCompositionStrategy_GivenCompositionContext_ReturnsCompositionStrategy()
    {
        // arrange
        var mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();

        _mockChordChoiceRepositoryFactory.Create(Arg.Any<CompositionConfiguration>()).Returns(mockChordChoiceRepository);

        mockChordChoiceRepository.Count.Returns(500);

        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        // act
        var compositionStrategy = _compositionStrategyFactory.Create(compositionConfiguration);

        // assert
        compositionStrategy.Should().NotBeNull();
    }
}

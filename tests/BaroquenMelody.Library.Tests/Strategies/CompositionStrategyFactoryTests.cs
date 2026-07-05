using BaroquenMelody.Infrastructure.Random;
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
    private ICompositionRule _mockCompositionRule = null!;

    private ILogger _mockLogger = null!;

    private CompositionStrategyFactory _compositionStrategyFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionStrategyFactory = new CompositionStrategyFactory(new NoteChoiceGenerator(), _mockCompositionRule, new ThreadLocalRandomProvider(), _mockLogger);
    }

    [Test]
    public void CreateCompositionStrategy_GivenCompositionContext_ReturnsCompositionStrategy()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        // act
        var compositionStrategy = _compositionStrategyFactory.Create(compositionConfiguration);

        // assert
        compositionStrategy.Should().NotBeNull();
    }
}

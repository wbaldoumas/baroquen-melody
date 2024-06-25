using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Strategies;

[TestFixture]
internal sealed class CompositionStrategyFactoryTests
{
    private IChordChoiceRepositoryFactory _mockChordChoiceRepositoryFactory = null!;

    private ICompositionRule _mockCompositionRule = null!;

    private CompositionStrategyFactory _compositionStrategyFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockChordChoiceRepositoryFactory = Substitute.For<IChordChoiceRepositoryFactory>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();

        _compositionStrategyFactory = new CompositionStrategyFactory(_mockChordChoiceRepositoryFactory, _mockCompositionRule);
    }

    [Test]
    public void CreateCompositionStrategy_GivenCompositionContext_ReturnsCompositionStrategy()
    {
        // arrange
        var mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();

        _mockChordChoiceRepositoryFactory.Create(Arg.Any<CompositionConfiguration>()).Returns(mockChordChoiceRepository);

        mockChordChoiceRepository.Count.Returns(500);

        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55.ToNote(), 90.ToNote()),
                new(Voice.Alto, 45.ToNote(), 80.ToNote())
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        // act
        var compositionStrategy = _compositionStrategyFactory.Create(compositionConfiguration);

        // assert
        compositionStrategy.Should().NotBeNull();
    }
}

using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;

namespace BaroquenMelody.Library.Tests.Strategies;

/// <summary>
///     Verifies that <see cref="CompositionConfiguration.MaxLookAheadDepth"/> is threaded through the
///     <see cref="CompositionStrategyFactory"/> into the <see cref="CompositionStrategy"/> and meaningfully changes
///     look-ahead behavior (a deeper search discovers dead-ends a shallow search cannot see), while still producing
///     valid compositions end-to-end.
/// </summary>
[TestFixture]
internal sealed class LookAheadDepthTests
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

        _compositionStrategyFactory = new CompositionStrategyFactory(_mockChordChoiceRepositoryFactory, _mockCompositionRule, new ThreadLocalRandomProvider(), _mockLogger);
    }

    [Test]
    public void Create_ThreadsConfiguredMaxLookAheadDepth_SoDeeperSearchPrunesChoicesAShallowSearchKeeps()
    {
        // arrange: a chord graph that is valid one chord ahead but dead-ends once a third chord is appended.
        // Every choice keeps every note in place (oblique, no scale-step change), so the only thing that varies
        // between the two strategies under test is the configured look-ahead depth.
        var mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();
        mockChordChoiceRepository.Count.Returns(new BigInteger(3));

        var configuration = TestCompositionConfigurations.Get();

        var obliqueChordChoice = new ChordChoice(configuration.Instruments.Select(static instrument => new NoteChoice(instrument, NoteMotion.Oblique, 0)));
        mockChordChoiceRepository.GetChordChoice(Arg.Any<BigInteger>()).Returns(obliqueChordChoice);

        _mockChordChoiceRepositoryFactory.Create(Arg.Any<CompositionConfiguration>()).Returns(mockChordChoiceRepository);

        // Chords remain valid while at most one chord has been appended to the preceding context (look-ahead depth one),
        // but every choice becomes invalid once a second chord is appended (look-ahead depth two reaches the dead-end).
        _mockCompositionRule
            .Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(static callInfo => callInfo.ArgAt<IReadOnlyList<BaroquenChord>>(0).Count <= 2);

        var precedingChords = new List<BaroquenChord>
        {
            _compositionStrategyFactory.Create(configuration).GenerateInitialChord()
        };

        // act
        var shallowChoices = _compositionStrategyFactory
            .Create(configuration with { MaxLookAheadDepth = 1 })
            .GetPossibleChordChoices(precedingChords);

        var deepChoices = _compositionStrategyFactory
            .Create(configuration with { MaxLookAheadDepth = 2 })
            .GetPossibleChordChoices(precedingChords);

        // assert
        shallowChoices.Should().NotBeEmpty("a depth-one look-ahead never sees the dead-end three chords away");
        deepChoices.Should().BeEmpty("a depth-two look-ahead discovers the dead-end and prunes every choice, proving the configured depth was threaded into the strategy");
    }

    [Test]
    public void Compose_WithADeeperLookAhead_StillProducesAValidComposition()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false, MaxLookAheadDepth = 2 };

        // act
        var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, 12345));

        // assert
        notes.Should().NotBeEmpty("a deeper look-ahead must still complete a full composition without painting itself into a corner");
    }
}

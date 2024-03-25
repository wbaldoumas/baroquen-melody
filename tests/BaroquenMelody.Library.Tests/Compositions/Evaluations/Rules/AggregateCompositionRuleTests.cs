using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations.Rules;

[TestFixture]
internal sealed class AggregateCompositionRuleTests
{
    private ICompositionRule _mockCompositionRule1 = null!;

    private ICompositionRule _mockCompositionRule2 = null!;

    private AggregateCompositionRule aggregateCompositionRule = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionRule1 = Substitute.For<ICompositionRule>();
        _mockCompositionRule2 = Substitute.For<ICompositionRule>();

        aggregateCompositionRule = new AggregateCompositionRule([_mockCompositionRule1, _mockCompositionRule2]);
    }
}

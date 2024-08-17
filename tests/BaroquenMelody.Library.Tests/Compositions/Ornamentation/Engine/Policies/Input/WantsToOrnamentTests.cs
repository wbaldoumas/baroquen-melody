using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Random;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class WantsToOrnamentTests
{
    private IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = null!;

    [SetUp]
    public void SetUp() => _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();

    [Test]
    public void ShouldProcess_WhenProbabilityHigherThanRandomNumber_ReturnsContinue()
    {
        // arrange
        var policy = new WantsToOrnament(_weightedRandomBooleanGenerator, 101);

        var ornamentationItem = new OrnamentationItem(Instrument.One, [], new Beat(new BaroquenChord([])), null);

        // act
        var result = policy.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(InputPolicyResult.Continue);
    }

    [Test]
    public void ShouldProcess_WhenProbabilityLowerThanRandomNumber_ReturnsReject()
    {
        // arrange
        var policy = new WantsToOrnament(_weightedRandomBooleanGenerator, -1);

        var ornamentationItem = new OrnamentationItem(Instrument.One, [], new Beat(new BaroquenChord([])), null);

        // act
        var result = policy.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(InputPolicyResult.Reject);
    }
}

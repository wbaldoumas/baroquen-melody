using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

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

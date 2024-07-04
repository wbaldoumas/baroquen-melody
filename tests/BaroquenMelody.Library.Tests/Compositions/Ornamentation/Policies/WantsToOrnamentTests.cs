using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Policies;

[TestFixture]
internal sealed class WantsToOrnamentTests
{
    [Test]
    public void ShouldProcess_WhenProbabilityHigherThanRandomNumber_ReturnsContinue()
    {
        // arrange
        var policy = new WantsToOrnament(101);

        var ornamentationItem = new OrnamentationItem(Voice.Soprano, [], new Beat(new BaroquenChord([])), null);

        // act
        var result = policy.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(InputPolicyResult.Continue);
    }

    [Test]
    public void ShouldProcess_WhenProbabilityLowerThanRandomNumber_ReturnsReject()
    {
        // arrange
        var policy = new WantsToOrnament(-1);

        var ornamentationItem = new OrnamentationItem(Voice.Soprano, [], new Beat(new BaroquenChord([])), null);

        // act
        var result = policy.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(InputPolicyResult.Reject);
    }
}

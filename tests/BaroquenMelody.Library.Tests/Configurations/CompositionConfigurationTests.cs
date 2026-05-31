using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configurations;

[TestFixture]
internal sealed class CompositionConfigurationTests
{
    [Test]
    public void ShuffleOrnamentationProcessors_DefaultsToTrue()
    {
        // arrange / act
        var configuration = TestCompositionConfigurations.Get();

        // assert
        configuration.ShuffleOrnamentationProcessors.Should().BeTrue();
    }
}

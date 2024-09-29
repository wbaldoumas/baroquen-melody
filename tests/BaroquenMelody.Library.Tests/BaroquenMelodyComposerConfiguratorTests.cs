using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

[TestFixture]
internal sealed class BaroquenMelodyComposerConfiguratorTests
{
    private BaroquenMelodyComposerConfigurator _baroquenMelodyComposerConfigurator = null!;

    [SetUp]
    public void SetUp() => _baroquenMelodyComposerConfigurator = new BaroquenMelodyComposerConfigurator(Substitute.For<ILogger<BaroquenMelody>>(), Substitute.For<IDispatcher>());

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Configure_returns_configured_BaroquenMelodyComposer_which_can_compose_a_BaroquenMelody(CompositionConfiguration compositionConfiguration)
    {
        // arrange
        var baroquenMelodyComposer = _baroquenMelodyComposerConfigurator.Configure(compositionConfiguration);

        // act
        var baroquenMelody = baroquenMelodyComposer.Compose(CancellationToken.None);

        // assert
        baroquenMelody.Should().NotBeNull();
        baroquenMelody.MidiFile.Should().NotBeNull();
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(TestCompositionConfigurations.Get(2, 10));

            yield return new TestCaseData(TestCompositionConfigurations.Get(3, 10));
        }
    }
}

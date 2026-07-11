using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionConfigurationPanelTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Panel_renders_the_composition_configuration_card()
    {
        // act
        var component = _testContext.RenderComponent<CompositionConfigurationPanel>();

        // assert
        component.FindComponents<CompositionConfigurationCard>().Should().ContainSingle();
    }

    [Test]
    public void Reset_restores_the_default_composition_configuration()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationPanel>();

        component.FindAll("input[type=number]")[1].Change("90");

        // act
        component.ClickButtonByText("Reset");

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Tempo.Should().Be(120);
    }
}

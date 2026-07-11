using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class OrnamentationConfigurationPanelTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Panel_renders_a_card_per_configurable_ornamentation()
    {
        // arrange
        var configurableOrnamentationCount = _testContext.Services.GetRequiredService<IOrnamentationConfigurationService>().ConfigurableOrnamentations.Count();

        // act
        var component = _testContext.RenderComponent<OrnamentationConfigurationPanel>();

        // assert
        component.FindComponents<OrnamentationConfigurationCard>().Should().HaveCount(configurableOrnamentationCount);
    }

    [Test]
    public void Search_filters_the_ornamentation_cards()
    {
        // arrange
        var component = _testContext.RenderComponent<OrnamentationConfigurationPanel>();
        var totalCardCount = component.FindComponents<OrnamentationConfigurationCard>().Count;

        // act
        component.Find("input[type=text]").Input("Trill");

        // assert
        var filteredCardCount = component.FindComponents<OrnamentationConfigurationCard>().Count;

        filteredCardCount.Should().BeGreaterThan(0).And.BeLessThan(totalCardCount);
    }

    [Test]
    public void Search_without_matches_shows_an_alert()
    {
        // arrange
        var component = _testContext.RenderComponent<OrnamentationConfigurationPanel>();

        // act
        component.Find("input[type=text]").Input("no such ornamentation");

        // assert
        component.FindComponents<OrnamentationConfigurationCard>().Should().BeEmpty();
        component.Markup.Should().Contain("No ornamentations found");
    }

    [Test]
    public void Reset_restores_the_default_ornamentation_configurations()
    {
        // arrange
        var defaultConfiguration = AggregateOrnamentationConfiguration.Default.Configurations.First();
        var ornamentationType = defaultConfiguration.OrnamentationType;
        var component = _testContext.RenderComponent<OrnamentationConfigurationPanel>();
        var alteredStatus = defaultConfiguration.Status == ConfigurationStatus.Disabled ? ConfigurationStatus.Enabled : ConfigurationStatus.Disabled;

        _testContext.Dispatcher.Dispatch(new UpdateCompositionOrnamentationConfiguration(ornamentationType, alteredStatus, defaultConfiguration.Probability));

        // act
        component.ClickButtonByText("Reset");

        // assert
        _testContext.StateOf<CompositionOrnamentationConfigurationState>()[ornamentationType]!.Status.Should().Be(defaultConfiguration.Status);
    }
}

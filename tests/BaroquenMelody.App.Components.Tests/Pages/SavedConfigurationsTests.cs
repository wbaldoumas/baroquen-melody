using BaroquenMelody.App.Components.Pages;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using NUnit.Framework;
using Meter = BaroquenMelody.Library.Enums.Meter;

namespace BaroquenMelody.App.Components.Tests.Pages;

[TestFixture]
internal sealed class SavedConfigurationsTests
{
    private AppComponentsTestContext _testContext = null!;

    private ICompositionConfigurationPersistenceService _mockPersistenceService = null!;

    private IRenderedComponent<MudDialogProvider> _dialogProvider = null!;

    private ISnackbar Snackbar => _testContext.Services.GetRequiredService<ISnackbar>();

    [SetUp]
    public void SetUp()
    {
        _mockPersistenceService = Substitute.For<ICompositionConfigurationPersistenceService>();

        _testContext = new AppComponentsTestContext(services => services.AddSingleton(_mockPersistenceService));
        _dialogProvider = _testContext.RenderComponent<MudDialogProvider>();
    }

    [TearDown]
    public void TearDown()
    {
        _dialogProvider.Dispose();
        _testContext.Dispose();
    }

    [Test]
    public void Saved_configurations_are_listed_by_name()
    {
        // arrange
        SetUpSavedConfigurations("sonata.json", "fugue.json");

        // act
        var component = _testContext.RenderComponent<SavedConfigurations>();

        // assert
        component.Markup.Should().ContainAll("sonata", "fugue");
    }

    [Test]
    public void Empty_configuration_list_shows_a_message()
    {
        // arrange
        SetUpSavedConfigurations();

        // act
        var component = _testContext.RenderComponent<SavedConfigurations>();

        // assert
        component.Markup.Should().Contain("no configurations found");
    }

    [Test]
    public void Failing_to_load_configurations_shows_an_error_toast()
    {
        // arrange
        _mockPersistenceService.LoadConfigurationsAsync(Arg.Any<CancellationToken>()).Returns<IEnumerable<SavedCompositionConfiguration>>(_ => throw new IOException("disk error"));

        // act
        _testContext.RenderComponent<SavedConfigurations>();

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void Search_filters_the_listed_configurations()
    {
        // arrange
        SetUpSavedConfigurations("sonata.json", "fugue.json");

        var component = _testContext.RenderComponent<SavedConfigurations>();

        // act
        component.Find("input").Input("fug");

        // assert
        component.Markup.Should().Contain("fugue");
        component.Markup.Should().NotContain("sonata");
    }

    [Test]
    public void Loading_a_configuration_applies_it_and_navigates_home()
    {
        // arrange
        SetUpSavedConfigurations("fugue.json");

        var component = _testContext.RenderComponent<SavedConfigurations>();

        // act
        component.Find("a.mud-link").Click();

        // assert: the configuration's tonic reaches the store, its name is remembered, and the app navigates home
        _testContext.StateOf<CompositionConfigurationState>().TonicNote.Should().Be(NoteName.G);
        _testContext.StateOf<SavedCompositionConfigurationState>().LastLoadedConfigurationName.Should().Be("fugue");

        var navigationManager = _testContext.Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();

        navigationManager.Uri.Should().Be(navigationManager.BaseUri);
    }

    [Test]
    public async Task Deleting_a_configuration_asks_for_confirmation_first()
    {
        // arrange
        SetUpSavedConfigurations("fugue.json", "sonata.json");
        _mockPersistenceService.DeleteConfigurationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var component = _testContext.RenderComponent<SavedConfigurations>();

        // act: the second icon button in the first row is delete
        component.FindAll("td button")[1].Click();

        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Delete existing configuration?"));

        _dialogProvider.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();

        // assert: the deleted configuration disappears while the other remains
        component.WaitForAssertion(() => component.Markup.Should().NotContain("fugue"));
        component.Markup.Should().Contain("sonata");
        await _mockPersistenceService.Received(1).DeleteConfigurationAsync("fugue.json", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cancelling_the_delete_confirmation_keeps_the_configuration()
    {
        // arrange
        SetUpSavedConfigurations("fugue.json");

        var component = _testContext.RenderComponent<SavedConfigurations>();

        // act
        component.FindAll("td button")[1].Click();

        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Delete existing configuration?"));

        _dialogProvider.FindAll("button").Single(button => button.TextContent.Trim() == "Cancel").Click();

        // assert
        component.Markup.Should().Contain("fugue");
        await _mockPersistenceService.DidNotReceive().DeleteConfigurationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void Failed_deletion_shows_an_error_toast()
    {
        // arrange
        SetUpSavedConfigurations("fugue.json");
        _mockPersistenceService.DeleteConfigurationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var component = _testContext.RenderComponent<SavedConfigurations>();

        // act
        component.FindAll("td button")[1].Click();

        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Delete existing configuration?"));

        _dialogProvider.FindAll("button").Single(button => button.TextContent.Trim() == "Delete").Click();

        // assert
        component.WaitForAssertion(() => Snackbar.ShownSnackbars.Should().ContainSingle());
        component.Markup.Should().Contain("fugue");
    }

    private void SetUpSavedConfigurations(params string[] fileNames)
    {
        var savedConfigurations = fileNames.Select(fileName => new SavedCompositionConfiguration(
            BuildConfiguration(),
            new FileInfo(fileName)
        ));

        _mockPersistenceService.LoadConfigurationsAsync(Arg.Any<CancellationToken>()).Returns(savedConfigurations.ToList());
    }

    private CompositionConfiguration BuildConfiguration() => new(
        _testContext.StateOf<InstrumentConfigurationState>().AllConfigurations,
        PhrasingConfiguration.Default,
        _testContext.StateOf<CompositionRuleConfigurationState>().Aggregate,
        _testContext.StateOf<CompositionOrnamentationConfigurationState>().Aggregate,
        NoteName.G,
        Mode.Dorian,
        Meter.ThreeFour,
        Meter.ThreeFour.DefaultMusicalTimeSpan(),
        MinimumMeasures: 30,
        Tempo: 100
    );
}

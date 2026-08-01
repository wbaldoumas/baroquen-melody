using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.App.Components.Tests.TestData;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class SaveCompositionConfigurationDialogTests
{
    private AppComponentsTestContext _testContext = null!;

    private ICompositionConfigurationPersistenceService _mockPersistenceService = null!;

    private IRenderedComponent<MudDialogProvider> _dialogProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _mockPersistenceService = Substitute.For<ICompositionConfigurationPersistenceService>();
        _mockPersistenceService.DoesConfigurationExist(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _mockPersistenceService.SaveConfigurationAsync(Arg.Any<CompositionConfiguration>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

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
    public async Task Save_is_disabled_until_a_valid_name_is_entered()
    {
        // arrange
        await ShowDialogAsync();

        SaveButton().HasAttribute("disabled").Should().BeTrue();

        // act
        _dialogProvider.Find("input").Input("My Configuration");

        // assert
        SaveButton().HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public async Task Invalid_file_name_characters_keep_save_disabled()
    {
        // arrange
        await ShowDialogAsync();

        // act
        _dialogProvider.Find("input").Input("bad/name");

        // assert
        SaveButton().HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public async Task Saving_a_new_configuration_persists_it_and_remembers_the_name()
    {
        // arrange
        var dialogReference = await ShowDialogAsync();

        _dialogProvider.Find("input").Input("My Configuration");

        // act
        SaveButton().Click();

        // assert
        var result = await dialogReference.Result;

        result!.Data.Should().Be(true);

        await _mockPersistenceService.Received(1).SaveConfigurationAsync(Arg.Any<CompositionConfiguration>(), "My Configuration", Arg.Any<CancellationToken>());

        _testContext.StateOf<SavedCompositionConfigurationState>().LastLoadedConfigurationName.Should().Be("My Configuration");
    }

    [Test]
    public async Task Saving_persists_the_ground_bass_form_and_pinned_pattern()
    {
        // arrange: the persisted configuration is what a future load maps back into the store, so it
        // must carry the form, the pinned pattern, and the modulation toggle - not just the fields the
        // dialog displays.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.SelectGroundBassPattern(_testContext, GroundBass.CadentialGround);
        GroundBassScenarios.SetGroundBassModulate(_testContext, modulate: false);

        CompositionConfiguration? savedConfiguration = null;

        _mockPersistenceService.SaveConfigurationAsync(
            Arg.Do<CompositionConfiguration>(configuration => savedConfiguration = configuration),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        ).Returns(true);

        await ShowDialogAsync();

        _dialogProvider.Find("input").Input("My Configuration");

        // act
        SaveButton().Click();

        // assert
        _dialogProvider.WaitForAssertion(() => savedConfiguration.Should().NotBeNull());

        savedConfiguration!.GroundBassConfiguration.Should().NotBeNull();
        savedConfiguration.GroundBassConfiguration!.Enabled.Should().BeTrue();
        savedConfiguration.GroundBassConfiguration.Pattern.Should().Be(GroundBass.CadentialGround);
        savedConfiguration.GroundBassConfiguration.Modulate.Should().BeFalse("the user's disabled journey must survive the save");
    }

    [Test]
    public async Task Existing_configuration_asks_before_overwriting_and_cancel_aborts()
    {
        // arrange
        _mockPersistenceService.DoesConfigurationExist(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        await ShowDialogAsync();

        _dialogProvider.Find("input").Input("My Configuration");

        // act
        SaveButton().Click();

        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Configuration already exists"));

        _dialogProvider
            .FindAll("button")
            .Last(button => button.TextContent.Trim() == "Cancel")
            .Click();

        // assert
        await _mockPersistenceService.DidNotReceive().SaveConfigurationAsync(Arg.Any<CompositionConfiguration>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Existing_configuration_is_overwritten_after_confirmation()
    {
        // arrange
        _mockPersistenceService.DoesConfigurationExist(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        await ShowDialogAsync();

        _dialogProvider.Find("input").Input("My Configuration");

        // act
        SaveButton().Click();

        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Configuration already exists"));

        _dialogProvider
            .FindAll("button")
            .Single(button => button.TextContent.Trim() == "Overwrite")
            .Click();

        // assert
        _dialogProvider.WaitForAssertion(() => _mockPersistenceService.Received(1).SaveConfigurationAsync(Arg.Any<CompositionConfiguration>(), "My Configuration", Arg.Any<CancellationToken>()).GetAwaiter().GetResult());
    }

    private async Task<IDialogReference> ShowDialogAsync()
    {
        var dialogService = _testContext.Services.GetRequiredService<IDialogService>();

        return await _dialogProvider.InvokeAsync(() => dialogService.ShowAsync<SaveCompositionConfigurationDialog>("Save composition configuration?"));
    }

    private AngleSharp.Dom.IElement SaveButton() => _dialogProvider
        .FindAll("button")
        .Single(button => button.TextContent.Trim() == "Save");
}

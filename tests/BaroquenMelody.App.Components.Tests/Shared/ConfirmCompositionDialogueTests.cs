using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class ConfirmCompositionDialogueTests
{
    private AppComponentsTestContext _testContext = null!;

    private IRenderedComponent<MudDialogProvider> _dialogProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _testContext = new AppComponentsTestContext();
        _dialogProvider = _testContext.RenderComponent<MudDialogProvider>();
    }

    [TearDown]
    public void TearDown()
    {
        _dialogProvider.Dispose();
        _testContext.Dispose();
    }

    [Test]
    public async Task Dialog_asks_whether_to_save_the_previous_composition()
    {
        // act
        await ShowDialogAsync();

        // assert
        _dialogProvider.Markup.Should().Contain("Previous composition has not yet been saved");
    }

    [Test]
    public async Task Yes_closes_with_a_save_request()
    {
        // arrange
        var dialogReference = await ShowDialogAsync();

        // act
        ClickButton("Yes");

        // assert
        var result = await dialogReference.Result;

        result!.Canceled.Should().BeFalse();
        result.Data.Should().Be(true);
    }

    [Test]
    public async Task No_closes_without_a_save_request()
    {
        // arrange
        var dialogReference = await ShowDialogAsync();

        // act
        ClickButton("No");

        // assert
        var result = await dialogReference.Result;

        result!.Canceled.Should().BeFalse();
        result.Data.Should().Be(false);
    }

    [Test]
    public async Task Cancel_cancels_the_dialog()
    {
        // arrange
        var dialogReference = await ShowDialogAsync();

        // act
        ClickButton("Cancel");

        // assert
        var result = await dialogReference.Result;

        result!.Canceled.Should().BeTrue();
    }

    private async Task<IDialogReference> ShowDialogAsync()
    {
        var dialogService = _testContext.Services.GetRequiredService<IDialogService>();

        return await _dialogProvider.InvokeAsync(() => dialogService.ShowAsync<ConfirmCompositionDialogue>("Save composition?"));
    }

    private void ClickButton(string text) => _dialogProvider
        .FindAll("button")
        .Single(button => button.TextContent.Trim() == text)
        .Click();
}

using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class AboutTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void About_dialog_displays_the_application_info()
    {
        // arrange
        _testContext.MockApplicationInfo.Version.Returns("1.2.3");
        _testContext.MockApplicationInfo.Commit.Returns("abc1234");
        _testContext.MockApplicationInfo.CommitDate.Returns("2026-07-11");
        _testContext.MockApplicationInfo.Sha.Returns("abc1234def5678");
        _testContext.MockApplicationInfo.RepositoryUrl.Returns("https://example.test/repo");
        _testContext.MockApplicationInfo.SupportUrl.Returns("https://example.test/support");
        _testContext.MockApplicationInfo.ContributeUrl.Returns("https://example.test/contribute");
        _testContext.MockApplicationInfo.LicenseUrl.Returns("https://example.test/license");

        var dialogProvider = _testContext.RenderComponent<MudDialogProvider>();
        var dialogService = _testContext.Services.GetRequiredService<IDialogService>();

        // act
        dialogProvider.InvokeAsync(() => dialogService.ShowAsync<About>()).GetAwaiter().GetResult();

        // assert
        dialogProvider.Markup.Should().ContainAll(
            "1.2.3",
            "abc1234",
            "2026-07-11",
            "https://example.test/repo/tree/abc1234def5678",
            "https://example.test/support",
            "https://example.test/contribute",
            "https://example.test/license"
        );
    }
}

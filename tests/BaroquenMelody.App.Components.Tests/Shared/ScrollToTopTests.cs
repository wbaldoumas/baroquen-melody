using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class ScrollToTopTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Renders_a_scroll_to_top_button()
    {
        // act
        var component = _testContext.RenderComponent<ScrollToTop>();

        // assert
        component.Find("button").ClassList.Should().Contain("mud-fab");
    }
}

using BaroquenMelody.App.Components.Layout;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Layout;

[TestFixture]
internal sealed class NavMenuTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Nav_menu_links_to_home_and_saved_configurations()
    {
        // act
        var component = _testContext.RenderComponent<NavMenu>();

        // assert
        var links = component.FindAll("a.mud-nav-link");

        links.Should().HaveCount(2);
        links[0].TextContent.Should().Contain("Home");
        links[1].TextContent.Should().Contain("Saved Configurations");
        links[1].GetAttribute("href").Should().Be("configurations");
    }
}

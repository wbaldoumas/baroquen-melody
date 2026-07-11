using BaroquenMelody.App.Components.Layout;
using Bunit;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Layout;

[TestFixture]
internal sealed class MainLayoutTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext(renderPopoverProvider: false);

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Layout_renders_the_app_bar_and_navigation_drawer()
    {
        // act
        var component = _testContext.RenderComponent<MainLayout>();

        // assert
        component.FindAll(".mud-appbar").Should().ContainSingle();
        component.FindComponents<NavMenu>().Should().ContainSingle();
    }

    [Test]
    public void Menu_button_toggles_the_drawer()
    {
        // arrange
        var component = _testContext.RenderComponent<MainLayout>();

        component.Find(".mud-drawer").ClassList.Should().Contain("mud-drawer--closed");

        // act
        component.Find(".mud-appbar button").Click();

        // assert
        component.Find(".mud-drawer").ClassList.Should().Contain("mud-drawer--open");
    }

    [Test]
    public void Dark_mode_button_toggles_the_theme()
    {
        // arrange
        var component = _testContext.RenderComponent<MainLayout>();

        // act: the dark-light mode button follows the menu button (the GitHub icon renders as a link)
        component.FindAll(".mud-appbar button")[1].Click();

        // assert
        _testContext.MockThemeProvider.Received(1).ToggleDarkMode();
    }

    [Test]
    public void About_menu_item_opens_the_about_dialog()
    {
        // arrange
        _testContext.MockApplicationInfo.Version.Returns("1.2.3");

        var component = _testContext.RenderComponent<MainLayout>();

        // act: open the more-options menu, then choose about
        component.FindAll(".mud-appbar button")[2].Click();

        component.WaitForAssertion(() => component
            .FindAll(".mud-menu-item")
            .Should().Contain(menuItem => menuItem.TextContent.Contains("About", StringComparison.Ordinal)));

        component
            .FindAll(".mud-menu-item")
            .Single(menuItem => menuItem.TextContent.Contains("About", StringComparison.Ordinal))
            .Click();

        // assert: the layout's own dialog provider hosts the about dialog
        component.WaitForAssertion(() => component.Markup.Should().Contain("1.2.3"));
    }
}

using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class AutoClosePopoverTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Popover_content_is_rendered_when_open()
    {
        // act
        RenderPopover(isPopoverOpen: true, () => { });

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("test popover content");
    }

    [Test]
    public void Overlay_is_not_rendered_when_closed()
    {
        // act
        RenderPopover(isPopoverOpen: false, () => { });

        // assert: the overlay renders under the popover provider
        _testContext.PopoverProvider.FindAll("div.mud-overlay").Should().BeEmpty();
    }

    [Test]
    public void Closing_the_overlay_invokes_the_popover_closed_callback()
    {
        // arrange
        var wasClosed = false;
        RenderPopover(isPopoverOpen: true, () => wasClosed = true);

        // act: the overlay renders under the popover provider
        _testContext.PopoverProvider.Find("div.mud-overlay").Click();

        // assert
        wasClosed.Should().BeTrue();
    }

    private IRenderedComponent<AutoClosePopover> RenderPopover(bool isPopoverOpen, Action onPopoverClosed) => _testContext.RenderComponent<AutoClosePopover>(parameters => parameters
        .Add(component => component.PopoverContent, "<span>test popover content</span>")
        .Add(component => component.IsPopoverOpen, isPopoverOpen)
        .Add(component => component.OnPopoverClosed, onPopoverClosed)
    );
}

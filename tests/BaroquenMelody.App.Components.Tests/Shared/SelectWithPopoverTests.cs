using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class SelectWithPopoverTests
{
    private static readonly string[] _items = ["alpha", "beta", "gamma"];

    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Renders_the_label_and_the_provided_value()
    {
        // act
        var component = RenderSelect(() => "beta", _ => { });

        // assert
        component.Markup.Should().Contain("test label");
        component.Find("input").GetAttribute("value").Should().Be("beta");
    }

    [Test]
    public void Clicking_the_adornment_opens_the_popover()
    {
        // arrange
        var component = RenderSelect(() => "beta", _ => { });

        // act
        component.Find("button").Click();

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("select popover content");
    }

    [Test]
    public void Selecting_an_item_reports_the_new_value()
    {
        // arrange
        var reportedValue = string.Empty;
        var component = RenderSelect(() => "beta", value => reportedValue = value);
        var select = component.FindComponent<MudSelect<string>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync("gamma")).GetAwaiter().GetResult();

        // assert
        reportedValue.Should().Be("gamma");
    }

    [Test]
    public void Items_are_displayed_using_the_display_converter()
    {
        // arrange
        var component = RenderSelect(() => "beta", _ => { });
        var select = component.FindComponent<MudSelect<string>>();

        // act
        component.InvokeAsync(() => select.Instance.OpenMenu()).GetAwaiter().GetResult();

        // assert
        _testContext.PopoverProvider.Markup.Should().ContainAll("ALPHA", "BETA", "GAMMA");
    }

    private IRenderedComponent<SelectWithPopover<string>> RenderSelect(Func<string> valueProvider, Action<string> onValueChanged) => _testContext.RenderComponent<SelectWithPopover<string>>(parameters => parameters
        .Add(component => component.PopoverContent, "<span>select popover content</span>")
        .Add(component => component.Label, "test label")
        .Add(component => component.ValueProvider, valueProvider)
        .Add(component => component.Items, _items)
        .Add(component => component.ValueChanged, onValueChanged)
        .Add(component => component.ConvertToDisplay, item => item.ToUpperInvariant())
    );
}

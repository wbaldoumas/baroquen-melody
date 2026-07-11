using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class NumericInputWithPopoverTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Renders_the_label_and_the_provided_value()
    {
        // act
        var component = RenderNumericInput(valueProvider: () => 42, _ => { });

        // assert
        component.Markup.Should().Contain("test label");
        component.Find("input").GetAttribute("value").Should().Be("42");
    }

    [Test]
    public void Changing_the_input_reports_the_new_value()
    {
        // arrange
        var reportedValue = 0;
        var component = RenderNumericInput(() => 42, value => reportedValue = value);

        // act
        component.Find("input").Change("55");

        // assert
        reportedValue.Should().Be(55);
    }

    [Test]
    public void Input_is_disabled_when_requested()
    {
        // act
        var component = RenderNumericInput(() => 42, _ => { }, isDisabled: true);

        // assert
        component.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Clicking_the_adornment_opens_the_popover()
    {
        // arrange
        var component = RenderNumericInput(() => 42, _ => { });

        // act
        component.Find("button").Click();

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("numeric popover content");
    }

    private IRenderedComponent<NumericInputWithPopover<int>> RenderNumericInput(Func<int> valueProvider, Action<int> onValueChanged, bool isDisabled = false) => _testContext.RenderComponent<NumericInputWithPopover<int>>(parameters => parameters
        .Add(component => component.PopoverContent, "<span>numeric popover content</span>")
        .Add(component => component.Label, "test label")
        .Add(component => component.ValueProvider, valueProvider)
        .Add(component => component.ValueChanged, onValueChanged)
        .Add(component => component.Min, 0)
        .Add(component => component.Max, 100)
        .Add(component => component.IsDisabled, isDisabled)
    );
}

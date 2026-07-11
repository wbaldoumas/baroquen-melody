using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class AutocompleteWithPopoverTests
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
        var component = RenderAutocomplete(() => "cello", _ => { });

        // assert
        component.Markup.Should().Contain("test label");
        component.Find("input").GetAttribute("value").Should().Be("CELLO");
    }

    [Test]
    public void Input_is_disabled_when_requested()
    {
        // act
        var component = RenderAutocomplete(() => "cello", _ => { }, isDisabled: true);

        // assert
        component.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Clicking_the_adornment_opens_the_popover()
    {
        // arrange
        var component = RenderAutocomplete(() => "cello", _ => { });

        // act: target the adornment button specifically - the clearable autocomplete renders a clear button too
        component.Find("div.mud-input-adornment button").Click();

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("autocomplete popover content");
    }

    [Test]
    public void Choosing_a_value_reports_the_new_value()
    {
        // arrange
        var reportedValue = string.Empty;
        var component = RenderAutocomplete(() => "cello", value => reportedValue = value);
        var autocomplete = component.FindComponent<MudAutocomplete<string>>();

        // act
        component.InvokeAsync(() => autocomplete.Instance.ValueChanged.InvokeAsync("violin")).GetAwaiter().GetResult();

        // assert
        reportedValue.Should().Be("violin");
    }

    private IRenderedComponent<AutocompleteWithPopover<string>> RenderAutocomplete(Func<string> valueProvider, Action<string> onValueChanged, bool isDisabled = false) => _testContext.RenderComponent<AutocompleteWithPopover<string>>(parameters => parameters
        .Add(component => component.PopoverContent, "<span>autocomplete popover content</span>")
        .Add(component => component.Label, "test label")
        .Add(component => component.ValueProvider, valueProvider)
        .Add(component => component.ToStringFunc, value => value.ToUpperInvariant())
        .Add(component => component.SearchFunc, (_, _) => Task.FromResult<IEnumerable<string>>(["violin", "cello"]))
        .Add(component => component.ValueChanged, onValueChanged)
        .Add(component => component.IsDisabled, isDisabled)
    );
}

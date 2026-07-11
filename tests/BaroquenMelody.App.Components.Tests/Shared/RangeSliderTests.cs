using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class RangeSliderTests
{
    private static readonly string[] _tickMarkLabels = ["low", "mid", "high"];

    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Range_slider_renders_an_upper_and_a_lower_input()
    {
        // act
        var component = RenderRangeSlider();

        // assert
        component.FindAll("input.mud-slider-input").Should().HaveCount(2);
    }

    [Test]
    public void Non_range_slider_renders_a_single_input()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.Range, false));

        // assert
        component.FindAll("input.mud-slider-input").Should().HaveCount(1);
    }

    [Test]
    public void Sliding_the_lower_thumb_reports_the_new_value()
    {
        // arrange
        var reportedValue = 0;
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.ValueChanged, value => reportedValue = value));

        // act: the upper input renders first, the lower input second
        component.FindAll("input.mud-slider-input")[1].Input("30");

        // assert
        reportedValue.Should().Be(30);
    }

    [Test]
    public void Sliding_the_upper_thumb_reports_the_new_upper_value()
    {
        // arrange
        var reportedUpperValue = 0;
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.UpperValueChanged, value => reportedUpperValue = value));

        // act
        component.FindAll("input.mud-slider-input")[0].Input("70");

        // assert
        reportedUpperValue.Should().Be(70);
    }

    [Test]
    public void Lower_value_is_clamped_below_the_upper_value_by_the_minimum_distance()
    {
        // arrange
        var reportedValue = 0;
        var component = RenderRangeSlider(parameters => parameters
            .Add(slider => slider.MinDistance, 5)
            .Add(slider => slider.ValueChanged, value => reportedValue = value)
        );

        // act: push the lower value into the upper value's territory
        component.SetParametersAndRender(parameters => parameters.Add(slider => slider.Value, 49));

        // assert
        reportedValue.Should().Be(45);
    }

    [Test]
    public void Upper_value_is_clamped_above_the_lower_value_by_the_minimum_distance()
    {
        // arrange
        var reportedUpperValue = 0;
        var component = RenderRangeSlider(parameters => parameters
            .Add(slider => slider.MinDistance, 5)
            .Add(slider => slider.UpperValueChanged, value => reportedUpperValue = value)
        );

        // act: push the upper value into the lower value's territory
        component.SetParametersAndRender(parameters => parameters.Add(slider => slider.UpperValue, 12));

        // assert
        reportedUpperValue.Should().Be(15);
    }

    [Test]
    public void Upper_value_is_raised_to_the_maximum_slideable_minimum()
    {
        // arrange
        var reportedUpperValue = 0;
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.UpperValueChanged, value => reportedUpperValue = value));

        // act
        component.SetParametersAndRender(parameters => parameters.Add(slider => slider.MaxSlideableMin, 60));

        // assert
        reportedUpperValue.Should().Be(60);
    }

    [Test]
    public void Lower_value_is_lowered_to_the_minimum_slideable_maximum()
    {
        // arrange
        var reportedValue = 0;
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.ValueChanged, value => reportedValue = value));

        // act
        component.SetParametersAndRender(parameters => parameters.Add(slider => slider.MinSlideableMax, 5));

        // assert
        reportedValue.Should().Be(5);
    }

    [Test]
    public void Valid_lower_and_upper_values_are_taken_as_is()
    {
        // arrange
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.Display, true));

        // act: both updates respect the minimum distance and slideable bounds
        component.SetParametersAndRender(parameters => parameters.Add(slider => slider.Value, 20));
        component.SetParametersAndRender(parameters => parameters.Add(slider => slider.UpperValue, 80));

        // assert
        component.Find(".mud-range-display").TextContent.Should().Be("20 - 80");
    }

    [Test]
    public void Tick_marks_are_rendered_for_each_step()
    {
        // act
        var component = RenderRangeSlider(
            parameters => parameters
                .Add(slider => slider.TickMarks, true)
                .Add(slider => slider.Step, 1),
            max: 10,
            upperValue: 8
        );

        // assert
        component.FindAll("span.mud-slider-track-tick").Should().HaveCount(11);
    }

    [Test]
    public void Tick_mark_labels_are_rendered_when_provided()
    {
        // act
        var component = RenderRangeSlider(
            parameters => parameters
                .Add(slider => slider.TickMarks, true)
                .Add(slider => slider.Step, 1)
                .Add(slider => slider.TickMarkLabels, _tickMarkLabels),
            value: 0,
            upperValue: 2,
            max: 2
        );

        // assert
        var labels = component.FindAll(".mud-slider-track-tick-label");

        labels.Should().HaveCount(3);
        labels[0].TextContent.Should().Be("low");
        labels[2].TextContent.Should().Be("high");
    }

    [Test]
    public void Display_shows_the_full_range_when_no_values_are_set()
    {
        // act
        var component = RenderRangeSlider(
            parameters => parameters.Add(slider => slider.Display, true),
            value: 0,
            upperValue: 100
        );

        // assert
        component.Find(".mud-range-display").TextContent.Should().Be("0 - 100");
    }

    [Test]
    public void Display_shows_the_selected_range()
    {
        // act
        var component = RenderRangeSlider(
            parameters => parameters.Add(slider => slider.Display, true),
            value: 20,
            upperValue: 60
        );

        // assert
        component.Find(".mud-range-display").TextContent.Should().Be("20 - 60");
    }

    [Test]
    public void Display_uses_the_custom_display_text_provider_when_set()
    {
        // act
        var component = RenderRangeSlider(
            parameters => parameters
                .Add(slider => slider.Display, true)
                .Add(slider => slider.DisplayTextProvider, (lower, upper) => $"from {lower} to {upper}"),
            value: 20,
            upperValue: 60
        );

        // assert
        component.Find(".mud-range-display").TextContent.Should().Be("from 20 to 60");
    }

    [Test]
    public void Value_labels_use_the_custom_label_text_when_provided()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters
            .Add(slider => slider.ValueLabel, true)
            .Add(slider => slider.LabelText, "lower label")
            .Add(slider => slider.UpperLabelText, "upper label")
        );

        // assert
        var labels = component.FindAll(".mud-slider-value-label");

        labels.Should().HaveCount(2);
        labels[0].TextContent.Trim().Should().Be("upper label");
        labels[1].TextContent.Trim().Should().Be("lower label");
    }

    [Test]
    public void Disabled_slider_disables_both_inputs()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.Disabled, true));

        // assert
        component.FindAll("input.mud-slider-input").Should().AllSatisfy(input => input.HasAttribute("disabled").Should().BeTrue());
    }

    [Test]
    public void Disabling_the_minimum_disables_only_the_lower_input()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.DisableMin, true));

        // assert
        var inputs = component.FindAll("input.mud-slider-input");

        inputs[0].HasAttribute("disabled").Should().BeFalse();
        inputs[1].HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Disabling_the_maximum_disables_only_the_upper_input()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.DisableMax, true));

        // assert
        var inputs = component.FindAll("input.mud-slider-input");

        inputs[0].HasAttribute("disabled").Should().BeTrue();
        inputs[1].HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public void Child_content_is_rendered()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.ChildContent, "<span>slider title</span>"));

        // assert
        component.Markup.Should().Contain("slider title");
    }

    [Test]
    public void Vertical_slider_gets_the_vertical_class()
    {
        // act
        var component = RenderRangeSlider(parameters => parameters.Add(slider => slider.Vertical, true));

        // assert
        component.Find("div.mud-slider").ClassList.Should().Contain("mud-slider-vertical");
    }

    private IRenderedComponent<RangeSlider<int>> RenderRangeSlider(
        Action<ComponentParameterCollectionBuilder<RangeSlider<int>>>? configure = null,
        int value = 10,
        int upperValue = 50,
        int min = 0,
        int max = 100) => _testContext.RenderComponent<RangeSlider<int>>(parameters =>
    {
        parameters
            .Add(slider => slider.Value, value)
            .Add(slider => slider.UpperValue, upperValue)
            .Add(slider => slider.Min, min)
            .Add(slider => slider.Max, max);

        configure?.Invoke(parameters);
    });
}

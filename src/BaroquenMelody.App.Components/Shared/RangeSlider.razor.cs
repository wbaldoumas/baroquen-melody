﻿/*
 * A RangeSlider based on the MudBlazor extensions MudRangeSlider. The main difference
 * is that this component allows for setting a minimum value for the upper value and a
 * maximum value for the lower value.
 */

// ReSharper disable CompareOfFloatsByEqualityOperator
#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable MA0011 // Specify IFormatProvider
#pragma warning disable CA1716 // Identifiers should not match keywords

using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.State;
using MudBlazor.Utilities;
using MudExtensions;
using System.Globalization;
using System.Numerics;

namespace BaroquenMelody.App.Components.Shared;

/// <summary>
///     Mud slider with range abilities.
/// </summary>
/// <typeparam name="T">The data type for the slider.</typeparam>
public partial class RangeSlider<T> : MudComponentBase
    where T : struct, INumber<T>
{
    private readonly ParameterState<T> _value;

    private readonly ParameterState<T> _upperValue;

    private readonly ParameterState<T?> _slideableMin;

    private readonly ParameterState<T?> _slideableMax;

    public RangeSlider()
    {
        using var registerScope = CreateRegisterScope();

        _value = registerScope.RegisterParameter<T>(nameof(Value))
            .WithParameter(() => Value)
            .WithEventCallback(() => ValueChanged)
            .WithChangeHandler(OnValueParameterChanged);

        _upperValue = registerScope.RegisterParameter<T>(nameof(UpperValue))
            .WithParameter(() => UpperValue)
            .WithEventCallback(() => UpperValueChanged)
            .WithChangeHandler(OnUpperValueParameterChanged);

        _slideableMin = registerScope.RegisterParameter<T?>(nameof(MaxSlideableMin))
            .WithParameter(() => MaxSlideableMin)
            .WithChangeHandler(OnSlideableMinChanged);

        _slideableMax = registerScope.RegisterParameter<T?>(nameof(MinSlideableMax))
            .WithParameter(() => MinSlideableMax)
            .WithChangeHandler(OnSlideableMaxChanged);
    }

    /// <summary>
    ///     If this is a Range Slider.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public bool Range { get; set; } = true;

    /// <summary>
    ///     Custom text for ValueLabel.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public string? LabelText { get; set; }

    /// <summary>
    ///     Custom text for upper ValueLabel.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public string? UpperLabelText { get; set; }

    /// <summary>
    ///     The minimum allowed value of the slider. Should not be equal to max.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public T Min { get; set; } = T.Zero;

    /// <summary>
    ///     The minimum value the upper slider thumb has.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public T? MaxSlideableMin { get; set; }

    /// <summary>
    ///     The maximum allowed value of the slider. Should not be equal to min.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public T Max { get; set; } = T.CreateTruncating(100);

    /// <summary>
    /// The maximum value the lower slider thumb has.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public T? MinSlideableMax { get; set; }

    /// <summary>
    ///     The minimum distance between the upper and lower values.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public T MinDistance { get; set; } = T.One;

    /// <summary>
    ///     How many steps the slider should take on each move.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Validation)]
    public T Step { get; set; } = T.One;

    /// <summary>
    ///     If true, the slider will be disabled.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    ///     If true and <seealso cref="Range"/>, the slider's min value will be disabled.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Behavior)]
    public bool DisableMin { get; set; }

    /// <summary>
    ///     If true and <seealso cref="Range"/>, the slider's max value will be disabled.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Behavior)]
    public bool DisableMax { get; set; }

    /// <summary>
    ///     Child content of component.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    [Category(CategoryTypes.Slider.Behavior)]
    public Converter<T?> Converter { get; set; } = new DefaultConverter<T?>() { Culture = CultureInfo.InvariantCulture };

    /// <summary>
    ///     Fires when value changed.
    /// </summary>
    [Parameter]
    public EventCallback<T> ValueChanged { get; set; }

    /// <summary>
    ///     Fires when upper value changed.
    /// </summary>
    [Parameter]
    public EventCallback<T> UpperValueChanged { get; set; }

    /// <summary>
    ///     Value of the component.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Data)]
    public T Value { get; set; } = T.Zero;

    /// <summary>
    ///     If range set, holds the higher value.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Data)]
    public T UpperValue { get; set; } = T.CreateTruncating(50);

    /// <summary>
    ///     The color of the component. It supports the Primary, Secondary and Tertiary theme colors.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public Color Color { get; set; } = Color.Primary;

    /// <summary>
    ///     If true, displays the Values below the slider.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public bool Display { get; set; }

    /// <summary>
    ///     If true, the dragging the slider will update the Value immediately.
    ///     If false, the Value is updated only on releasing the handle.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Behavior)]
    public bool Immediate { get; set; } = true;

    /// <summary>
    ///     If true, displays the slider vertical.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public bool Vertical { get; set; }

    /// <summary>
    ///     If true, displays tick marks on the track.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public bool TickMarks { get; set; }

    /// <summary>
    ///     Labels for tick marks, will attempt to map the labels to each step in index order.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public string[]? TickMarkLabels { get; set; }

    /// <summary>
    ///     Labels for tick marks, will attempt to map the labels to each step in index order.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public Size Size { get; set; } = Size.Small;

    /// <summary>
    ///     The variant to use.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Button.Appearance)]
    public Variant Variant { get; set; } = Variant.Text;

    /// <summary>
    ///     Displays the value over the slider thumb.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Button.Appearance)]
    public bool ValueLabel { get; set; }

    /// <summary>
    ///     A func to generate <see cref="DisplayText"/> below.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Slider.Appearance)]
    public Func<T, T, string>? DisplayTextProvider { get; set; }

    private int _tickMarkCount;

    private async Task OnValueParameterChanged()
    {
        if (Range && Convert.ToDecimal(_value.Value) + Convert.ToDecimal(MinDistance) >= Convert.ToDecimal(_upperValue.Value))
        {
            await _value.SetValueAsync(_upperValue.Value - MinDistance);
        }

        if (_slideableMax.Value != null && _value.Value > _slideableMax.Value)
        {
            await _value.SetValueAsync((T)_slideableMax.Value);
        }
    }

    private async Task OnUpperValueParameterChanged()
    {
        if (Range && Convert.ToDecimal(_upperValue.Value) - Convert.ToDecimal(MinDistance) <= Convert.ToDecimal(_value.Value))
        {
            await _upperValue.SetValueAsync(_value.Value + MinDistance);
        }

        if (_slideableMin.Value != null && _slideableMin.Value > _upperValue.Value)
        {
            await _upperValue.SetValueAsync((T)_slideableMin.Value);
        }
    }

    private async Task OnSlideableMinChanged()
    {
        if (_slideableMin.Value != null && _upperValue.Value < _slideableMin.Value)
        {
            await _upperValue.SetValueAsync((T)_slideableMin.Value);
        }
    }

    private async Task OnSlideableMaxChanged()
    {
        if (_slideableMax.Value != null && _slideableMax.Value < _value.Value)
        {
            await _value.SetValueAsync((T)_slideableMax.Value);
        }
    }

    protected string Classname =>
        new CssBuilder("mud-slider")
            .AddClass($"mud-slider-{EnumExtensions.ToDescriptionString(Size)}")
            .AddClass($"mud-slider-{EnumExtensions.ToDescriptionString(Color)}")
            .AddClass("mud-slider-vertical", Vertical)
            .AddClass(Class)
            .Build();

    protected string? DisplayText
    {
        get
        {
            if (DisplayTextProvider != null)
            {
                return DisplayTextProvider(_value.Value, _upperValue.Value);
            }

            if (!Range)
            {
                return Converter.Set(_value.Value);
            }

            // if both lower and upper are not set then it is any
            if (Convert.ToDouble(_value.Value) == Convert.ToDouble(Min) &&
                (Convert.ToDouble(_upperValue.Value) == Convert.ToDouble(Max) || Convert.ToDouble(_upperValue.Value) == 0))
            {
                return $"{Min} - {Max}";
            }

            var displayText = $"{_value.Value} - {_upperValue.Value}";

            // If lower is min or not defined
            if (Convert.ToDouble(_value.Value) == Convert.ToDouble(Min))
            {
                displayText = $"{Min} - {_upperValue.Value}";
            }

            // If upper is max or not defined
            if (Convert.ToDouble(_upperValue.Value) == Convert.ToDouble(Max) || Convert.ToDouble(_upperValue.Value) == 0)
            {
                displayText = $"{_value.Value} - {Max}";
            }

            return displayText;
        }
    }

    protected override void OnParametersSet()
    {
        if (TickMarks)
        {
            var min = Convert.ToDouble(Min);
            var max = Convert.ToDouble(Max);
            var step = Convert.ToDouble(Step);

            _tickMarkCount = 1 + (int)((max - min) / step);
        }

        base.OnParametersSet();
    }

    private double CalculateWidth()
    {
        var min = Convert.ToDouble(Min);
        var max = Convert.ToDouble(Max);
        var value = Convert.ToDouble(_value.Value);

        if (Range)
        {
            value = Convert.ToDouble(_upperValue.Value) + min - Convert.ToDouble(_value.Value);
        }

        var result = 100.0 * (value - min) / (max - min);
        result = Math.Min(Math.Max(0, result), 100);

        return Math.Round(result, 2);
    }

    private double CalculateLeft()
    {
        var min = Convert.ToDouble(Min);
        var max = Convert.ToDouble(Max);
        var value = Convert.ToDouble(_value.Value);
        var result = 100.0 * (value - min) / (max - min);
        result = Math.Min(Math.Max(0, result), 100);

        return Math.Round(result, 2);
    }

    private string GetValueText => _value.Value.ToString(format: null, CultureInfo.InvariantCulture);

    private string GetUpperValueText => _upperValue.Value.ToString(format: null, CultureInfo.InvariantCulture);

    private async Task SetValueTextAsync(string? text)
    {
        if (T.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            await _value.SetValueAsync(result);
        }
    }

    private async Task SetUpperValueTextAsync(string? text)
    {
        if (T.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            await _upperValue.SetValueAsync(result);
        }
    }
}

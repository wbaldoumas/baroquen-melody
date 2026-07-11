using AngleSharp.Html.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace BaroquenMelody.App.Components.Tests;

internal static class RenderedFragmentExtensions
{
    // Re-render before clicking: when a dispatch (or a child interaction) has re-rendered child components,
    // clicking through a stale markup snapshot silently no-ops in bUnit.
    public static void ClickButtonByText<TComponent>(this IRenderedComponent<TComponent> component, string text)
        where TComponent : IComponent
    {
        component.Render();
        component.FindAll("button").First(button => button.TextContent.Contains(text, StringComparison.Ordinal)).Click();
    }

    // CardHeaderSwitch renders the desktop enable and lock switches first, then their mobile duplicates.
    public static IHtmlInputElement EnableSwitch(this IRenderedFragment component) => (IHtmlInputElement)component.FindAll("input.mud-switch-input")[0];

    public static IHtmlInputElement LockSwitch(this IRenderedFragment component) => (IHtmlInputElement)component.FindAll("input.mud-switch-input")[1];
}

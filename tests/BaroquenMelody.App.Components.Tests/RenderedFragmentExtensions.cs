using AngleSharp.Html.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BaroquenMelody.App.Components.Tests;

internal static class RenderedFragmentExtensions
{
    // Re-render before clicking: when a dispatch (or a child interaction) has re-rendered child components,
    // clicking through a stale markup snapshot silently no-ops in bUnit. Then wait for the click's handler:
    // bUnit's synchronous Click() discards the dispatch task, and the renderer queues the handler behind any
    // render queued from another thread (MudBlazor's popover service batches on a timer and re-renders the
    // popover provider the test context hosts), so a test that read state straight after Click() could read
    // it before the click had happened. A handler that awaits user interaction (a dialog) would block here;
    // trigger those with ClickAsync and WaitForAssertion.
    public static void ClickButtonByText<TComponent>(this IRenderedComponent<TComponent> component, string text)
        where TComponent : IComponent
    {
        component.Render();

        component
            .FindAll("button")
            .First(button => button.TextContent.Contains(text, StringComparison.Ordinal))
            .ClickAsync(new MouseEventArgs())
            .GetAwaiter()
            .GetResult();
    }

    // CardHeaderSwitch renders the desktop enable and lock switches first, then their mobile duplicates.
    public static IHtmlInputElement EnableSwitch(this IRenderedFragment component) => (IHtmlInputElement)component.FindAll("input.mud-switch-input")[0];

    public static IHtmlInputElement LockSwitch(this IRenderedFragment component) => (IHtmlInputElement)component.FindAll("input.mud-switch-input")[1];
}

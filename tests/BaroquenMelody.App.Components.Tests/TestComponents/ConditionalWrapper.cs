using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BaroquenMelody.App.Components.Tests.TestComponents;

/// <summary>
///     Wraps child content behind a flag so tests can mount and unmount a component through the renderer,
///     exercising its disposal path the same way a real tab switch or navigation would.
/// </summary>
internal sealed class ConditionalWrapper : ComponentBase
{
    [Parameter]
    public bool Show { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Show)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}

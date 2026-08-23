using ArchUnitNET.Domain;
using ArchUnitNET.NUnit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 2: structure of the Razor class library. ArchUnitNET sees compiled Razor components as classes
///     deriving from ComponentBase (code-behind partials merge into the same type).
/// </summary>
[TestFixture]
internal sealed class FrontendStructureTests
{
    private static readonly Architecture Architecture = BaroquenMelodyArchitecture.Architecture;

    [Test]
    public void Components_reside_in_the_expected_namespaces()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Components)
            .And()
            .AreAssignableTo(typeof(ComponentBase))
            .Should()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.App\.Components(\.(Layout|Pages|Shared))?$")
            .Check(Architecture);
    }

    [Test]
    public void The_UI_assembly_defines_no_reducers_effects_or_action_subscribers()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Components)
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullNameMatching(@"^Fluxor\.(ReducerMethodAttribute|EffectMethodAttribute|IActionSubscriber)$")
            .Because("state transitions live in the Library's Store; the UI only reads IState<T> and dispatches actions")
            .Check(Architecture);
    }

    [Test]
    public void Components_subscribing_to_state_changes_are_async_disposable()
    {
        // CallAny cannot see calls to the generic ObserveChanges<TState> extension (the call site references
        // a closed generic instance, the architecture holds the open declaration), so the rule keys off the
        // type-level dependency on the extension class instead.
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Components)
            .And()
            .DependOnAny(Types().That().HaveFullName("BaroquenMelody.Infrastructure.State.StateExtensions"))
            .Should()
            .ImplementInterface(typeof(IAsyncDisposable))
            .Because("manual ObserveChanges subscriptions must be disposed; FluxorComponent's own disposal does not cover them")
            .Check(Architecture);
    }
}

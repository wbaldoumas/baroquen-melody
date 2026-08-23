using ArchUnitNET.Domain;
using ArchUnitNET.NUnit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 2: structure of the Razor class library. ArchUnitNET sees compiled Razor components as classes
///     implementing IComponent (code-behind partials merge into the same type). Assignability to a base class
///     is only resolved through LOADED assemblies, so "derives from ComponentBase" misses every component
///     whose base (FluxorComponent, MudComponentBase, LayoutComponentBase) lives in an unloaded package;
///     the interface Razor stamps on every component is the reliable subject predicate.
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
            .ImplementInterface(typeof(IComponent))
            .Should()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.App\.Components(\.(Layout|Pages|Shared))?$")
            .Because("components live in Layout, Pages or Shared; services and helpers live elsewhere")
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
    public void Components_subscribing_to_state_changes_declare_their_own_DisposeAsync()
    {
        // CallAny cannot see the call to the generic ObserveChanges<TState> extension (the call target is a
        // generic-instance member, not the open declaration MethodMembers() enumerates), so the subject is
        // keyed off the type-level dependency on the extension class. Every FluxorComponent child already
        // inherits IAsyncDisposable, so the condition asserts the component's OWN DisposeAsync override —
        // the thing that actually disposes the subscription.
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Components)
            .And()
            .DependOnAny(Types().That().HaveFullName("BaroquenMelody.Infrastructure.State.StateExtensions"))
            .Should()
            .HaveMethodMemberWithName("DisposeAsync()")
            .AndShould()
            .ImplementInterface(typeof(IAsyncDisposable))
            .Because("manual ObserveChanges subscriptions must be disposed; FluxorComponent's own disposal does not cover them")
            .Check(Architecture);
    }
}

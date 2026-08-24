using ArchUnitNET.Domain;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Positive controls: one deliberately failing rule per detection mechanism the catalog relies on, so a
///     silently vacuous rule (see the external-provider trap below) cannot masquerade as a passing one.
/// </summary>
[TestFixture]
internal sealed class PositiveControlTests
{
    private static readonly Architecture Architecture = BaroquenMelodyArchitecture.Architecture;

    [Test]
    public void Attribute_dependencies_are_detected()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .And()
            .ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Store\.(Reducers|Effects)$")
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullNameMatching(@"^Fluxor\.(ReducerMethodAttribute|EffectMethodAttribute)$");

        Assert.That(rule.HasNoViolations(Architecture), Is.False, "the reducers and effects carry these attributes; the F-2 mechanism must see them");
    }

    [Test]
    public void External_type_dependencies_are_detected()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Infrastructure)
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullName("System.Random");

        Assert.That(rule.HasNoViolations(Architecture), Is.False, "the two random seams construct System.Random; the L-11/L-12 mechanism must see them");
    }

    [Test]
    public void Cross_assembly_dependencies_are_detected()
    {
        var rule = Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .Should()
            .NotDependOnAny(Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Infrastructure));

        Assert.That(rule.HasNoViolations(Architecture), Is.False, "the Library depends on Infrastructure; the A-1..A-7 mechanism must see it");
    }

    [Test]
    public void Test_fixture_attributes_are_detected()
    {
        var rule = Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.LibraryTests)
            .And()
            .HaveAnyAttributes(typeof(TestFixtureAttribute))
            .Should()
            .BePublic();

        Assert.That(rule.HasNoViolations(Architecture), Is.False, "fixtures are internal; the T-1/T-2 subject predicate must select them");
    }

    [Test]
    public void An_object_provider_naming_an_external_type_is_silently_empty()
    {
        // Documented trap: Types()/Classes()/MethodMembers() enumerate LOADED assemblies only, so a provider that
        // names an external type matches nothing and NotDependOnAny(provider) passes vacuously. Only the
        // ...AnyTypesThat() condition form (or HaveFullName-style predicates on the target) reaches external types.
        var externalProvider = Types().That().HaveFullName("System.Random");

        var vacuousRule = Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Infrastructure)
            .Should()
            .NotDependOnAny(externalProvider);

        Assert.Multiple(() =>
        {
            Assert.That(externalProvider.GetObjects(Architecture), Is.Empty);
            Assert.That(vacuousRule.HasNoViolations(Architecture), Is.True, "vacuous pass — never write external-type bans in this form");
        });
    }
}

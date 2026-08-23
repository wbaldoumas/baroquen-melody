using ArchUnitNET.Domain;
using ArchUnitNET.NUnit;
using NUnit.Framework;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 1: cross-assembly dependency direction. These encode the dependency graph documented in
///     CLAUDE.md plus the package boundaries that are currently only implicit in the csproj files.
/// </summary>
[TestFixture]
internal sealed class LayerDependencyTests
{
    private static readonly Architecture Architecture = BaroquenMelodyArchitecture.Architecture;

    [Test]
    public void Infrastructure_must_not_depend_on_the_Library()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Infrastructure)
            .Should()
            .NotDependOnAny(Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Library))
            .Because("the dependency graph is Library -> Infrastructure, never the reverse")
            .Check(Architecture);
    }

    [Test]
    public void Infrastructure_must_not_depend_on_the_UI_assembly()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Infrastructure)
            .Should()
            .NotDependOnAny(Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Components))
            .Check(Architecture);
    }

    [Test]
    public void The_Library_must_not_depend_on_the_UI_assembly()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library)
            .Should()
            .NotDependOnAny(Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Components))
            .Check(Architecture);
    }

    [Test]
    public void The_console_app_must_not_depend_on_the_UI_assembly()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Console)
            .Should()
            .NotDependOnAny(Types().That().ResideInAssembly(BaroquenMelodyArchitecture.Components))
            .Check(Architecture);
    }

    [Test]
    public void The_UI_assembly_must_not_depend_on_MAUI()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Components)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Microsoft\.Maui")
            .Because("platform glue lives in the BaroquenMelody.App host, never in the Razor class library")
            .Check(Architecture);
    }

    [Test]
    public void Only_the_UI_assembly_may_depend_on_MudBlazor()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library, BaroquenMelodyArchitecture.Infrastructure, BaroquenMelodyArchitecture.Console)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^MudBlazor")
            .Check(Architecture);
    }

    [Test]
    public void Fluxor_Blazor_Web_stays_out_of_Library_and_Infrastructure()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.Library, BaroquenMelodyArchitecture.Infrastructure, BaroquenMelodyArchitecture.Console)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Fluxor\.Blazor")
            .Because("only the UI layer may bind Fluxor to Blazor; the Library uses Fluxor core alone")
            .Check(Architecture);
    }
}

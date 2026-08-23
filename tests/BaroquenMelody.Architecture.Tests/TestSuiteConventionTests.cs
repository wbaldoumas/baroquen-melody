using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Freeze;
using ArchUnitNET.NUnit;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 3: conventions of the test suites themselves.
/// </summary>
[TestFixture]
internal sealed class TestSuiteConventionTests
{
    private static readonly Architecture Architecture = BaroquenMelodyArchitecture.Architecture;

    private static readonly IType CompositionConfigurationType = Architecture.Types
        .First(candidate => string.Equals(candidate.FullName, "BaroquenMelody.Library.Configurations.CompositionConfiguration", StringComparison.Ordinal));

    [Test]
    public void Test_fixtures_are_named_with_a_Tests_suffix()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.LibraryTests, BaroquenMelodyArchitecture.InfrastructureTests, BaroquenMelodyArchitecture.ComponentsTests)
            .And()
            .HaveAnyAttributes(typeof(TestFixtureAttribute))
            .Should()
            .HaveNameEndingWith("Tests")
            .Check(Architecture);
    }

    [Test]
    public void Test_fixtures_are_internal_sealed()
    {
        Classes()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.LibraryTests, BaroquenMelodyArchitecture.InfrastructureTests, BaroquenMelodyArchitecture.ComponentsTests)
            .And()
            .HaveAnyAttributes(typeof(TestFixtureAttribute))
            .Should()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .Check(Architecture);
    }

    [Test]
    public void No_test_is_marked_Explicit_or_Ignore()
    {
        Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.LibraryTests, BaroquenMelodyArchitecture.InfrastructureTests, BaroquenMelodyArchitecture.ComponentsTests)
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullNameMatching(@"^NUnit\.Framework\.(ExplicitAttribute|IgnoreAttribute)$")
            .Because("the suite has zero Explicit/Ignore usage today; quarantined tests need a better home than an attribute")
            .Check(Architecture);
    }

    [Test]
    public void No_new_fixtures_construct_CompositionConfiguration_outside_TestData()
    {
        // Roughly ten existing fixtures call the primary constructor directly, so the rule is frozen:
        // the committed baseline holds today's violations and only NEW call sites fail the build.
        var rule = Types()
            .That()
            .ResideInAssembly(BaroquenMelodyArchitecture.LibraryTests)
            .And()
            .DoNotResideInNamespace("BaroquenMelody.Library.Tests.TestData")
            .Should()
            .NotCallAny(
                MethodMembers()
                    .That()
                    .AreConstructors()
                    .And()
                    .AreDeclaredIn(CompositionConfigurationType)
                    .And()
                    .DoNotHaveName(".ctor(BaroquenMelody.Library.Configurations.CompositionConfiguration)"))
            .Because("seeded tests build configurations through TestCompositionConfigurations.Get so determinism conventions stay in one place; the copy constructor is excluded because sealed-record with-clones compile to a direct copy-constructor call");

        FreezingArchRule.Freeze(rule, GetFrozenViolationsPath("composition-configuration-constructors.json")).Check(Architecture);
    }

    private static string GetFrozenViolationsPath(string fileName, [CallerFilePath] string sourceFilePath = "")
    {
        var directory = Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "FrozenViolations");
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, fileName);
    }
}

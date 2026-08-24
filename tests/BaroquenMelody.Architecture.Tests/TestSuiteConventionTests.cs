using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Freeze;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
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

    private static readonly string FrozenViolationsPath = GetFrozenViolationsPath("composition-configuration-constructors.json");

    private static readonly JsonViolationStore FrozenViolationsStore = new(FrozenViolationsPath);

    private static readonly TypesShouldConjunctionWithDescription CompositionConfigurationConstructorRule = BuildCompositionConfigurationConstructorRule();

    // Snapshot the committed baseline at type-initialisation time, BEFORE any frozen evaluation rewrites the
    // store, so the baseline-minimality check below sees what was committed, not what this run wrote.
    private static readonly IReadOnlyList<StringIdentifier> CommittedBaseline = FrozenViolationsStore
        .GetFrozenViolations(CompositionConfigurationConstructorRule)
        .ToList();

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
            .Because("every fixture in the repo is named <Subject>Tests; the suffix is how test files are found")
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
            .Because("fixtures are never inherited or exported; internal sealed is the repo-wide shape")
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
        // Seventeen existing fixtures call the primary constructor directly, so the rule is frozen: the committed
        // baseline holds today's violating TYPES and only new types fail. Freeze silently re-baselines when it
        // cannot find the rule (store missing, or the rule description — Because() text included — edited), so
        // that condition is asserted explicitly first.
        Assert.That(
            FrozenViolationsStore.RuleAlreadyFrozen(CompositionConfigurationConstructorRule),
            Is.True,
            $"no frozen baseline for this rule in {FrozenViolationsPath}: the file is missing or the rule description changed; re-freeze deliberately and commit the JSON");

        FreezingArchRule.Freeze(CompositionConfigurationConstructorRule, FrozenViolationsStore).Check(Architecture);
    }

    [Test]
    public void The_frozen_CompositionConfiguration_baseline_contains_only_current_violations()
    {
        // Freeze rewrites the store on every evaluation, so a fixed violation vanishes from the local file but
        // only reaches CI if the shrunk JSON is committed. Comparing the committed snapshot against the live
        // evaluation turns an un-committed shrink into a red test.
        Assert.That(CommittedBaseline, Is.Not.Empty, "the committed baseline is empty or unreadable — the frozen rule would silently re-baseline");

        var currentViolations = CompositionConfigurationConstructorRule
            .Evaluate(Architecture)
            .Where(result => !result.Passed)
            .Select(result => result.EvaluatedObjectIdentifier)
            .ToHashSet();

        var stale = CommittedBaseline.Where(identifier => !currentViolations.Contains(identifier)).ToList();

        Assert.That(stale, Is.Empty, $"baseline entries that no longer violate (rerun locally and commit the shrunk JSON): {string.Join(", ", stale)}");
    }

    private static TypesShouldConjunctionWithDescription BuildCompositionConfigurationConstructorRule()
    {
        return Types()
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
    }

    private static string GetFrozenViolationsPath(string fileName, [CallerFilePath] string sourceFilePath = "")
    {
        return Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "FrozenViolations", fileName);
    }
}

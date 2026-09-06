using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 3: Stryker's test universe. The Library's <c>stryker-config.json</c> runs the suite through a
///     <c>test-case-filter</c> that excludes the NUnit categories <c>TestCategories</c> declares: the seeded sweeps
///     (<c>Composition</c>) and the tests that compose a whole piece to pin one property (<c>WholeComposition</c>),
///     which cover nearly every mutant and were most of every mutant's test time. The filter must name exactly the
///     declared categories (a declared one missing from it puts its tests back into every covering mutant's run; one
///     nothing declares excludes nothing), the suite must tag exactly the declared names (a misspelt tag is invisible
///     to the filter), and the unit leg of <c>test.yml</c> must keep excluding only <c>Composition</c>: the
///     whole-composition tests leave Stryker's universe, not the suite, and excluded there too they would run nowhere.
/// </summary>
[TestFixture]
internal sealed class MutationUniverseTests
{
    private const string TestCategoriesTypeName = "BaroquenMelody.Library.Tests.TestCategories";

    private const string CompositionCategory = "Composition";

    private const string StrykerConfigKey = "stryker-config";

    private const string TestCaseFilterKey = "test-case-filter";

    private const string ExcludeClausePrefix = "TestCategory!=";

    private const string UnitFilterDeclaration = "const string UnitFilter = ";

    // The ways a category can be applied at run time rather than through an attribute; the guards' reflection
    // cannot see those, so the suite must not use them.
    private static readonly string[] RunTimeCategoryMarkers = [".SetCategory(", "Property(\"Category\""];

    private static readonly string[] BuildFolders = ["bin", "obj"];

    private static readonly string LibraryTestsPath = GetRepositoryPath(Path.Combine("tests", "BaroquenMelody.Library.Tests"));

    private static readonly string StrykerConfigPath = Path.Combine(LibraryTestsPath, "stryker-config.json");

    private static readonly string TestScriptPath = GetRepositoryPath(Path.Combine("scripts", "test.cs"));

    [Test]
    public void Stryker_excludes_exactly_the_declared_test_categories()
    {
        var declared = ReadDeclaredCategories();

        using var document = JsonDocument.Parse(File.ReadAllText(StrykerConfigPath));
        var filter = document.RootElement.GetProperty(StrykerConfigKey).GetProperty(TestCaseFilterKey).GetString();

        filter.Should().NotBeNullOrWhiteSpace("{0} must carry a `{1}`; without one Stryker runs the seeded sweeps against every mutant", StrykerConfigPath, TestCaseFilterKey);

        // `TestCategory!=A&TestCategory!=B`: every clause excludes one category and the clauses are joined with `&`.
        // The NUnit adapter honours category clauses unconditionally and silently drops any other filter that selects
        // more than 2,000 tests, which is why the filter is written in these terms and nothing else may join them.
        var clauses = filter!.Split('&');
        var malformed = clauses.Where(static clause => !clause.StartsWith(ExcludeClausePrefix, StringComparison.Ordinal) || clause.Length == ExcludeClausePrefix.Length).ToList();
        var excluded = clauses.Except(malformed, StringComparer.Ordinal).Select(static clause => clause[ExcludeClausePrefix.Length..]).Order(StringComparer.Ordinal).ToList();

        using var scope = new AssertionScope();

        malformed.Should().BeEmpty("every clause of `{0}` in {1} must be `{2}<category>`, joined with `&`; a name clause or another operator changes what the NUnit adapter honours", TestCaseFilterKey, StrykerConfigPath, ExcludeClausePrefix);
        excluded.Should().Equal(declared, "`{0}` in {1} must exclude exactly the categories {2} declares: a declared category missing from the filter runs its tests against every covering mutant, and a filtered category nothing declares excludes nothing", TestCaseFilterKey, StrykerConfigPath, TestCategoriesTypeName);
    }

    [Test]
    public void The_Library_suite_tags_exactly_the_declared_categories()
    {
        var declared = ReadDeclaredCategories();
        var tagged = BaroquenMelodyArchitecture.LibraryTests
            .GetTypes()
            .SelectMany(static type => TestCategoryReflection.OfFixture(type).Concat(TestCategoryReflection.TestsOf(type).SelectMany(TestCategoryReflection.OfTest)))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        using var scope = new AssertionScope();

        tagged.Except(declared, StringComparer.Ordinal).Should().BeEmpty("the Library suite's only categories are Stryker exclusions, by design ({0} says so): a category the suite tags but {0} does not declare is not in Stryker's filter, so its tests run against every covering mutant; declare it and add it to the filter, or fix the tag", TestCategoriesTypeName);
        declared.Except(tagged, StringComparer.Ordinal).Should().BeEmpty("a category {0} declares but no test carries is a filter clause that excludes nothing; tag the tests it was meant for or drop the constant", TestCategoriesTypeName);
    }

    [Test]
    public void No_category_is_applied_at_run_time()
    {
        // TestCategoryReflection reads attributes, the way the NUnit adapter's filter sees them; a category set at
        // run time (TestCaseData.SetCategory, a Property attribute named Category) is invisible to it, so a misspelt
        // one would slip past both guards. None exists today; keep it that way, or extend the reflection.
        var offenders = Directory
            .EnumerateFiles(LibraryTestsPath, "*.cs", SearchOption.AllDirectories)
            .Where(static file => !Path.GetRelativePath(LibraryTestsPath, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(static segment => BuildFolders.Contains(segment, StringComparer.OrdinalIgnoreCase)))
            .SelectMany(static file => File.ReadLines(file).Select((text, index) => (File: file, Line: index + 1, Text: text)))
            .Where(static entry => RunTimeCategoryMarkers.Any(marker => entry.Text.Contains(marker, StringComparison.Ordinal)))
            .Select(static entry => $"{Path.GetRelativePath(LibraryTestsPath, entry.File)}:{entry.Line}")
            .ToList();

        offenders.Should().BeEmpty("a category applied at run time is invisible to the attribute reflection the universe and shard guards use; apply categories through [Category] or the Category property of [TestFixture], [TestCase] or [TestCaseSource]");
    }

    [Test]
    public void The_unit_leg_excludes_only_the_Composition_category()
    {
        var declaration = File.ReadLines(TestScriptPath)
            .Select(static line => line.Trim())
            .FirstOrDefault(static line => line.StartsWith(UnitFilterDeclaration, StringComparison.Ordinal));

        declaration.Should().NotBeNull("the guard reads the `{0}\"...\";` line of {1}; if the script's shape changed, update this guard with it", UnitFilterDeclaration, TestScriptPath);
        declaration.Should().Be($"{UnitFilterDeclaration}\"{ExcludeClausePrefix}{CompositionCategory}\";", "the unit leg of test.yml runs everything but the Composition fixtures, which run in the composition shards; a whole-composition category excluded there as well would run nowhere on CI, since it leaves Stryker's universe, not the suite");
    }

    private static List<string> ReadDeclaredCategories()
    {
        var type = BaroquenMelodyArchitecture.LibraryTests.GetType(TestCategoriesTypeName);

        type.Should().NotBeNull("{0} declares the categories Stryker's filter excludes, one `public const string` each; the guard reads them from the Library.Tests assembly", TestCategoriesTypeName);

        var declared = type!
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(static field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(static field => (string)field.GetRawConstantValue()!)
            .Order(StringComparer.Ordinal)
            .ToList();

        declared.Should().Contain(CompositionCategory, "{0} must keep declaring the Composition category the shard map, the unit leg and Stryker's filter are built on", TestCategoriesTypeName);

        return declared;
    }

    private static string GetRepositoryPath(string relativePath, [CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "..", relativePath));
    }
}

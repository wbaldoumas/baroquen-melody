using NUnit.Framework;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 3: the CI shard map. <c>test.yml</c> runs the Library suite as one "unit" job (<c>TestCategory!=Composition</c>)
///     plus one job per key of <c>composition-shards.json</c>, whose fixture lists become <c>FullyQualifiedName</c>
///     filters. A <c>[Category("Composition")]</c> fixture missing from every shard would silently never run on CI,
///     one listed twice would run twice, and a shard key without a matrix entry would never be scheduled, so the
///     map must partition the tagged fixtures exactly and the workflow must name every shard.
/// </summary>
[TestFixture]
internal sealed class CompositionShardTests
{
    private const string CompositionCategory = "Composition";

    private const string UnitShard = "unit";

    private static readonly string ShardMapPath = GetRepositoryPath(Path.Combine(".github", "workflows", "composition-shards.json"));

    private static readonly string WorkflowPath = GetRepositoryPath(Path.Combine(".github", "workflows", "test.yml"));

    [Test]
    public void Every_Composition_fixture_is_listed_in_exactly_one_CI_shard()
    {
        var fixtures = GetCompositionFixtures().Select(static type => type.FullName!).Order(StringComparer.Ordinal).ToList();

        Assert.That(fixtures, Is.Not.Empty, "no [Category(\"Composition\")] fixture was found in Library.Tests: the discovery is broken, not the shard map");

        var listed = ReadShardMap()
            .SelectMany(static shard => shard.Value.Select(fixture => (Shard: shard.Key, Fixture: fixture)))
            .ToList();

        var unsharded = fixtures.Except(listed.Select(static entry => entry.Fixture), StringComparer.Ordinal).ToList();
        var unknown = listed.Select(static entry => entry.Fixture).Except(fixtures, StringComparer.Ordinal).ToList();
        var duplicated = listed
            .GroupBy(static entry => entry.Fixture, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => $"{group.Key} ({string.Join(", ", group.Select(static entry => entry.Shard))})")
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(unsharded, Is.Empty, $"Composition fixtures missing from {ShardMapPath}; without an entry they never run on CI");
            Assert.That(unknown, Is.Empty, $"entries in {ShardMapPath} that are not [Category(\"Composition\")] fixtures in Library.Tests (renamed, untagged, or a typo)");
            Assert.That(duplicated, Is.Empty, $"fixtures listed in more than one shard of {ShardMapPath}");
        });
    }

    [Test]
    public void Every_Composition_fixture_is_selectable_by_its_full_name()
    {
        // The workflow selects a shard's tests with `FullyQualifiedName~<full name>.`; a parameterized fixture
        // ([TestFixture(args)] or [TestFixtureSource], whose tests render as `Class(args).Method`), a generic one
        // or a nested one would be listed in the map, pass the partition check, and still never match on CI.
        var unselectable = GetCompositionFixtures()
            .Where(static type => type.IsNested
                || type.IsGenericTypeDefinition
                || type.GetCustomAttributes<TestFixtureSourceAttribute>(inherit: true).Any()
                || type.GetCustomAttributes<TestFixtureAttribute>(inherit: true).Any(static fixture => fixture.Arguments.Length > 0))
            .Select(static type => type.FullName!)
            .ToList();

        Assert.That(unselectable, Is.Empty, "Composition fixtures whose test names would not match a `FullyQualifiedName~<full name>.` filter (parameterized, sourced, generic or nested fixtures); keep sweep fixtures plain, or extend the shard filter");
    }

    [Test]
    public void The_Composition_category_is_applied_to_fixtures_not_to_individual_tests()
    {
        // The unit shard excludes the category and the composition shards select by fixture name, so a test method
        // tagged Composition inside an untagged fixture would be excluded by the former and selected by no shard.
        var methodLevelTags = BaroquenMelodyArchitecture.LibraryTests
            .GetTypes()
            .Where(static type => !HasCompositionCategory(type))
            .SelectMany(static type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(static method => method.GetCustomAttributes<CategoryAttribute>(inherit: true).Any(IsCompositionCategory))
            .Select(static method => $"{method.DeclaringType!.FullName}.{method.Name}")
            .ToList();

        Assert.That(methodLevelTags, Is.Empty, "test methods tagged [Category(\"Composition\")] inside fixtures that are not; tag the fixture (and add it to composition-shards.json) or the test never runs on CI");
    }

    [Test]
    public void Every_shard_in_the_map_is_a_matrix_entry_of_the_test_workflow()
    {
        var matrixEntries = ReadMatrixEntries();

        Assert.That(matrixEntries, Is.Not.Empty, $"no `- <name>` entries were found between `matrix:` and `steps:` in {WorkflowPath}; the workflow's shape changed, update this guard with it");

        var shards = ReadShardMap().Keys.ToList();
        var shardsWithoutAJob = shards.Except(matrixEntries, StringComparer.Ordinal).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(shardsWithoutAJob, Is.Empty, $"shard keys in {ShardMapPath} with no `- <shard>` matrix entry in {WorkflowPath}; a shard the matrix does not name never runs");
            Assert.That(shards, Does.Not.Contain(UnitShard), $"`{UnitShard}` is the workflow's category-filtered leg; fixtures listed under it in {ShardMapPath} would never run");
        });
    }

    private static IEnumerable<Type> GetCompositionFixtures() => BaroquenMelodyArchitecture.LibraryTests.GetTypes().Where(HasCompositionCategory);

    private static bool HasCompositionCategory(Type type) => type.GetCustomAttributes<CategoryAttribute>(inherit: true).Any(IsCompositionCategory);

    private static bool IsCompositionCategory(CategoryAttribute category) => string.Equals(category.Name, CompositionCategory, StringComparison.Ordinal);

    private static Dictionary<string, string[]> ReadShardMap()
    {
        var shards = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(ShardMapPath));

        Assert.That(shards, Is.Not.Null.And.Not.Empty, $"{ShardMapPath} must be a JSON object of shard name to fixture full names");

        return shards!;
    }

    /// <summary>
    ///     The `- name` lines of the matrix block: every line between the first `matrix:` and the following
    ///     `steps:` that, trimmed, starts with `- `. Comments (`# - name`) do not count.
    /// </summary>
    private static List<string> ReadMatrixEntries()
    {
        var lines = File.ReadAllLines(WorkflowPath).Select(static line => line.Trim()).ToList();
        var matrixStart = lines.FindIndex(static line => string.Equals(line, "matrix:", StringComparison.Ordinal));
        var stepsStart = matrixStart < 0 ? -1 : lines.FindIndex(matrixStart, static line => string.Equals(line, "steps:", StringComparison.Ordinal));

        if (matrixStart < 0 || stepsStart < 0)
        {
            return [];
        }

        return lines
            .Skip(matrixStart + 1)
            .Take(stepsStart - matrixStart - 1)
            .Where(static line => line.StartsWith("- ", StringComparison.Ordinal))
            .Select(static line => line[2..].Trim())
            .ToList();
    }

    private static string GetRepositoryPath(string relativePath, [CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "..", relativePath));
    }
}

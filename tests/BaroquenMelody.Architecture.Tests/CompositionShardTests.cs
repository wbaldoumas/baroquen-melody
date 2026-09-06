using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 3: the CI shard map. <c>test.yml</c> runs the Library suite as one "unit" job (<c>TestCategory!=Composition</c>)
///     plus one job per key of <c>composition-shards.json</c>, whose fixture lists become <c>FullyQualifiedName</c>
///     filters. A <c>[Category(TestCategories.Composition)]</c> fixture missing from every shard would silently never run on CI,
///     one listed twice would run twice, and a shard key without a matrix entry would never be scheduled, so the
///     map must partition the tagged fixtures exactly and the workflow must name every shard. Codecov, for its
///     part, must wait for exactly one upload per matrix entry before it posts. (<c>scripts/test.cs --verify-shards</c>
///     holds the behavioural half: that the filters built from the map select every test exactly once.)
/// </summary>
[TestFixture]
internal sealed class CompositionShardTests
{
    private const string CompositionCategory = "Composition";

    private const string UnitShard = "unit";

    private const string AfterNBuildsKey = "after_n_builds:";

    private static readonly string ShardMapPath = GetRepositoryPath(Path.Combine(".github", "workflows", "composition-shards.json"));

    private static readonly string WorkflowPath = GetRepositoryPath(Path.Combine(".github", "workflows", "test.yml"));

    private static readonly string CodecovPath = GetRepositoryPath("codecov.yml");

    [Test]
    public void Every_Composition_fixture_is_listed_in_exactly_one_CI_shard()
    {
        var fixtures = GetCompositionFixtures().Select(static type => type.FullName!).Order(StringComparer.Ordinal).ToList();

        fixtures.Should().NotBeEmpty("Library.Tests carries [Category(\"Composition\")] fixtures; finding none means the discovery is broken, not the shard map");

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

        using var scope = new AssertionScope();

        unsharded.Should().BeEmpty("a Composition fixture missing from {0} never runs on CI", ShardMapPath);
        unknown.Should().BeEmpty("every entry in {0} must be a [Category(\"Composition\")] fixture in Library.Tests (renamed, untagged, or a typo)", ShardMapPath);
        duplicated.Should().BeEmpty("a fixture listed in more than one shard of {0} runs more than once", ShardMapPath);
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

        unselectable.Should().BeEmpty("the test names of a parameterized, sourced, generic or nested fixture would not match a `FullyQualifiedName~<full name>.` filter; keep sweep fixtures plain, or extend the shard filter");
    }

    [Test]
    public void The_Composition_category_is_applied_to_fixtures_not_to_individual_tests()
    {
        // The unit shard excludes the category and the composition shards select by fixture name, so a test method
        // tagged Composition inside an untagged fixture would be excluded by the former and selected by no shard.
        var methodLevelTags = BaroquenMelodyArchitecture.LibraryTests
            .GetTypes()
            .Where(static type => !HasCompositionCategory(type))
            .SelectMany(TestCategoryReflection.TestsOf)
            .Where(static method => TestCategoryReflection.OfTest(method).Any(IsCompositionCategory))
            .Select(static method => $"{method.DeclaringType!.FullName}.{method.Name}")
            .ToList();

        methodLevelTags.Should().BeEmpty("a test method tagged [Category(\"Composition\")] inside an untagged fixture never runs on CI; tag the fixture (and add it to composition-shards.json)");
    }

    [Test]
    public void Every_shard_in_the_map_is_a_matrix_entry_of_the_test_workflow()
    {
        var matrixEntries = ReadMatrixEntries();

        matrixEntries.Should().NotBeEmpty("the guard reads the `- <name>` entries between `matrix:` and `steps:` in {0}; if the workflow's shape changed, update this guard with it", WorkflowPath);

        var shards = ReadShardMap().Keys.ToList();
        var shardsWithoutAJob = shards.Except(matrixEntries, StringComparer.Ordinal).ToList();

        using var scope = new AssertionScope();

        shardsWithoutAJob.Should().BeEmpty("a shard key in {0} with no `- <shard>` matrix entry in {1} never runs", ShardMapPath, WorkflowPath);
        shards.Should().NotContain(UnitShard, "`{0}` is the category-filtered leg that scripts/test.cs defines; fixtures listed under it in {1} would never run", UnitShard, ShardMapPath);
    }

    [Test]
    public void Codecov_waits_for_one_upload_per_matrix_entry()
    {
        // Codecov counts one upload per job (the unit job sends its three coverage files in one action call), so
        // every after_n_builds in codecov.yml must equal the matrix size: fewer, and statuses and the PR comment
        // post on partial coverage; more, and they never post.
        var matrixEntries = ReadMatrixEntries();
        var afterNBuilds = File.ReadLines(CodecovPath)
            .Select(static line => line.Trim())
            .Where(static line => line.StartsWith(AfterNBuildsKey, StringComparison.Ordinal))
            .Select(static line => int.Parse(line[AfterNBuildsKey.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture))
            .ToList();

        using var scope = new AssertionScope();

        afterNBuilds.Should().NotBeEmpty("{0} must hold statuses and the PR comment with `{1}` until every shard has uploaded", CodecovPath, AfterNBuildsKey);
        afterNBuilds.Should().AllBeEquivalentTo(matrixEntries.Count, "every `{0}` in {1} must equal the {2} matrix entries of {3}, one upload per job", AfterNBuildsKey, CodecovPath, matrixEntries.Count, WorkflowPath);
    }

    private static IEnumerable<Type> GetCompositionFixtures() => BaroquenMelodyArchitecture.LibraryTests.GetTypes().Where(HasCompositionCategory);

    private static bool HasCompositionCategory(Type type) => TestCategoryReflection.OfFixture(type).Any(IsCompositionCategory);

    private static bool IsCompositionCategory(string category) => string.Equals(category, CompositionCategory, StringComparison.Ordinal);

    private static Dictionary<string, string[]> ReadShardMap()
    {
        var shards = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(ShardMapPath));

        shards.Should().NotBeNullOrEmpty("{0} must be a JSON object of shard name to fixture full names", ShardMapPath);

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

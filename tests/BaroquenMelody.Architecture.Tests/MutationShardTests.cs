using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BaroquenMelody.ArchitectureTests;

/// <summary>
///     Tier 3: the mutation shard map. <c>mutation.yml</c> runs the full Library mutation as one job per key of
///     <c>mutation-shards.json</c>, whose folder lists <c>scripts/mutate.cs</c> turns into <c>-m</c> globs. A
///     top-level Library folder missing from every shard would never be mutated, one listed twice would be mutated
///     twice and counted twice in the merged report, and a shard key without a matrix entry would never be
///     scheduled, so the map must partition the Library's source folders exactly and the workflow must name every
///     shard.
/// </summary>
[TestFixture]
internal sealed class MutationShardTests
{
    private const string RootFolder = ".";

    private const string ShardListKey = "shard:";

    private static readonly string[] BuildFolders = ["bin", "obj"];

    private static readonly string LibraryPath = GetRepositoryPath(Path.Combine("src", "BaroquenMelody.Library"));

    private static readonly string ShardMapPath = GetRepositoryPath(Path.Combine(".github", "workflows", "mutation-shards.json"));

    private static readonly string WorkflowPath = GetRepositoryPath(Path.Combine(".github", "workflows", "mutation.yml"));

    [Test]
    public void Every_Library_source_folder_is_listed_in_exactly_one_mutation_shard()
    {
        var folders = GetSourceFolders();

        folders.Should().NotBeEmpty("{0} holds the Library's sources; finding no folder means the discovery is broken, not the shard map", LibraryPath);

        var listed = ReadShardMap()
            .SelectMany(static shard => shard.Value.Select(folder => (Shard: shard.Key, Folder: folder)))
            .ToList();

        var unsharded = folders.Except(listed.Select(static entry => entry.Folder), StringComparer.Ordinal).ToList();
        var unknown = listed.Select(static entry => entry.Folder).Except(folders, StringComparer.Ordinal).ToList();
        var duplicated = listed
            .GroupBy(static entry => entry.Folder, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => $"{group.Key} ({string.Join(", ", group.Select(static entry => entry.Shard))})")
            .ToList();

        using var scope = new AssertionScope();

        unsharded.Should().BeEmpty("a Library folder missing from {0} is never mutated by the full run", ShardMapPath);
        unknown.Should().BeEmpty("every entry in {0} must be a top-level folder of {1} that holds C# sources, or `{2}` for its root files", ShardMapPath, LibraryPath, RootFolder);
        duplicated.Should().BeEmpty("a folder listed in more than one shard of {0} is mutated twice and counted twice in the merged report", ShardMapPath);
    }

    [Test]
    public void Every_mutation_shard_is_a_matrix_entry_of_the_mutation_workflow()
    {
        var matrixShards = ReadMatrixShards();

        matrixShards.Should().NotBeEmpty("no `- <shard>` entries were found under `{0}` in {1}; the workflow's shape changed, update this guard with it", ShardListKey, WorkflowPath);

        var shards = ReadShardMap().Keys.ToList();

        using var scope = new AssertionScope();

        shards.Except(matrixShards, StringComparer.Ordinal).Should().BeEmpty("a shard key in {0} with no `- <shard>` entry under `{1}` in {2} never runs", ShardMapPath, ShardListKey, WorkflowPath);
        matrixShards.Except(shards, StringComparer.Ordinal).Should().BeEmpty("a `- <shard>` entry under `{0}` in {1} that is not a key of {2} fails at run time", ShardListKey, WorkflowPath, ShardMapPath);
    }

    /// <summary>
    ///     The top-level folders of the Library project that hold C# sources (build output excluded), plus
    ///     <c>.</c> when the project root holds sources itself.
    /// </summary>
    private static List<string> GetSourceFolders()
    {
        var folders = Directory
            .EnumerateDirectories(LibraryPath)
            .Where(static directory => !BuildFolders.Contains(Path.GetFileName(directory), StringComparer.OrdinalIgnoreCase))
            .Where(static directory => Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories).Any())
            .Select(static directory => Path.GetFileName(directory))
            .ToList();

        if (Directory.EnumerateFiles(LibraryPath, "*.cs", SearchOption.TopDirectoryOnly).Any())
        {
            folders.Add(RootFolder);
        }

        return folders.Order(StringComparer.Ordinal).ToList();
    }

    private static Dictionary<string, string[]> ReadShardMap()
    {
        var shards = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(ShardMapPath));

        shards.Should().NotBeNullOrEmpty("{0} must be a JSON object of shard name to Library folder names", ShardMapPath);

        return shards!;
    }

    /// <summary>
    ///     The `- name` lines directly under the workflow's `shard:` matrix key: every line after it that, trimmed,
    ///     starts with `- `, up to the first that does not. Comments (`# - name`) do not count.
    /// </summary>
    private static List<string> ReadMatrixShards()
    {
        var lines = File.ReadAllLines(WorkflowPath).Select(static line => line.Trim()).ToList();
        var listStart = lines.FindIndex(static line => string.Equals(line, ShardListKey, StringComparison.Ordinal));

        if (listStart < 0)
        {
            return [];
        }

        return lines
            .Skip(listStart + 1)
            .TakeWhile(static line => line.StartsWith("- ", StringComparison.Ordinal))
            .Select(static line => line[2..].Trim())
            .ToList();
    }

    private static string GetRepositoryPath(string relativePath, [CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "..", relativePath));
    }
}

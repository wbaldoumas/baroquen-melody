#!/usr/bin/env dotnet
// The one way to run the test suite, locally and on CI (.github/workflows/test.yml calls it), so the shard
// filters, the coverage settings, the hang bound and the results layout cannot drift between the two.
//
//   dotnet run scripts/test.cs                               whole suite: Library, Infrastructure, App.Components, Architecture
//   dotnet run scripts/test.cs -- --shard composition-b      one CI shard as its matrix job runs it: unit, or a key of composition-shards.json
//   dotnet run scripts/test.cs -- --verify-shards            prove the shard map partitions the Library suite, test for test
//   dotnet run scripts/test.cs -- --durations <trx|dir>...   per-fixture and per-test durations (balance the shards by runner time)
//
// Options: --coverage (coverlet, opencover, SingleHit: as CI) · --results <dir> (default TestResults; the per-run
// subfolders are wiped first so a stale file is never read as this run's) · --configuration <name> (default
// Release: the composition search runs ~1.9x faster than in Debug, and CI measures coverage in Release).
//
// Every run carries VSTest's hang bound (--blame-hang-timeout, HangTimeout below): an inactivity timer that
// resets whenever a test starts or finishes, so once nothing has happened for that long the test host is
// terminated (no dump) and a Sequence_*.xml next to the trx lists the tests that were in flight — a hang fails
// the run in minutes instead of holding the job until its own timeout.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

const string UnitShard = "unit";
const string UnitFilter = "TestCategory!=Composition";

// The NUnit adapter honours a category clause unconditionally but silently drops any other filter that selects
// more than this many tests, which would make an oversized name-filtered shard run the whole suite.
const int AdapterSelectLimit = 2000;

// Ten minutes without any test starting or finishing: the longest seeded sweep takes about a minute on the CI
// runner, so only a genuine hang reaches it. The --blame-* switches are VSTest's; opting global.json into
// Microsoft.Testing.Platform would mean replacing them with --hangdump / --hangdump-timeout.
const string HangTimeout = "10m";

const string Usage = """
    usage: dotnet run scripts/test.cs -- [mode] [options]

    modes (default: run the whole suite)
      --shard <name>            run one CI shard as its matrix job does: unit, or a key of composition-shards.json
      --verify-shards           run every shard and the unfiltered Library suite; fail unless the shards partition it
      --durations <trx|dir>...  summarise per-fixture and per-test durations from trx files or folders of them

    options
      --coverage                collect coverage as CI does (coverlet, opencover, SingleHit)
      --results <dir>           trx and coverage output, relative to the repository root (default TestResults)
      --configuration <name>    build configuration (default Release)
    """;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var repositoryRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourcePath())!, ".."));
var shardMapPath = Path.Combine(repositoryRoot, ".github", "workflows", "composition-shards.json");
var onGitHub = string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase);

var library = new Project("library", "tests/BaroquenMelody.Library.Tests", Coverable: true);
var otherProjects = new[]
{
    new Project("infrastructure", "tests/BaroquenMelody.Infrastructure.Tests", Coverable: true),
    new Project("components", "tests/BaroquenMelody.App.Components.Tests", Coverable: true),
    new Project("architecture", "tests/BaroquenMelody.Architecture.Tests", Coverable: false),
};

string? shard = null;
var coverage = false;
var verifyShards = false;
var durationInputs = new List<string>();
var results = "TestResults";
var configuration = "Release";

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--shard":
            if ((shard = TakeValue()) is null) return Fail("--shard needs a shard name");
            break;
        case "--coverage":
            coverage = true;
            break;
        case "--verify-shards":
            verifyShards = true;
            break;
        case "--durations":
            while (TakeValue() is { } input) durationInputs.Add(input);
            if (durationInputs.Count == 0) return Fail("--durations needs at least one trx file or folder");
            break;
        case "--results":
            if ((results = TakeValue()!) is null) return Fail("--results needs a directory");
            break;
        case "--configuration":
            if ((configuration = TakeValue()!) is null) return Fail("--configuration needs a name");
            break;
        case "--help" or "-h" or "-?":
            Console.WriteLine(Usage);
            return 0;
        default:
            return Fail($"unknown argument '{args[i]}'{Environment.NewLine}{Usage}");
    }

    string? TakeValue()
    {
        if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            return null;
        }

        return args[++i];
    }
}

var resultsRoot = Path.GetFullPath(Path.Combine(repositoryRoot, results));

if (durationInputs.Count > 0)
{
    return ReportDurations(durationInputs);
}

return verifyShards ? VerifyShards() : RunSuite(shard);

int RunSuite(string? shardName)
{
    var outcomes = new List<Outcome>();

    if (shardName is null)
    {
        outcomes.Add(RunProject(library, library.Name, filter: null, noBuild: false));
        outcomes.AddRange(otherProjects.Select(project => RunProject(project, project.Name, filter: null, noBuild: false)));
    }
    else
    {
        var filter = ShardFilter(shardName);

        if (filter is null)
        {
            return 2;
        }

        Console.WriteLine($"shard {shardName}: --filter \"{filter}\"");
        outcomes.Add(RunProject(library, library.Name, filter, noBuild: false));

        if (shardName == UnitShard)
        {
            outcomes.AddRange(otherProjects.Select(project => RunProject(project, project.Name, filter: null, noBuild: false)));
        }
    }

    return Summarize(outcomes);
}

int VerifyShards()
{
    var map = ReadShardMap();
    var shards = new List<string> { UnitShard };
    shards.AddRange(map.Keys);

    // The unfiltered run builds; the shard runs reuse that build. (--list-tests cannot stand in for the full run:
    // the adapter applies filters at execution, so only executed tests prove what a filter selects.)
    var full = RunProject(library, Path.Combine("verify", "all"), filter: null, noBuild: false);
    var perShard = new List<(string Shard, Outcome Outcome)>();

    foreach (var name in shards)
    {
        var filter = ShardFilter(name);

        if (filter is null)
        {
            return 2;
        }

        Console.WriteLine($"shard {name}: --filter \"{filter}\"");
        perShard.Add((name, RunProject(library, Path.Combine("verify", name), filter, noBuild: true)));
    }

    if (full.Trx is null || perShard.Any(entry => entry.Outcome.Trx is null))
    {
        return Fail("a run produced no trx file, so the partition cannot be checked; see the output above");
    }

    var fullTests = Trx.ExecutedTests(full.Trx);
    var membership = new Dictionary<string, List<string>>(StringComparer.Ordinal);

    foreach (var (name, outcome) in perShard)
    {
        foreach (var test in Trx.ExecutedTests(outcome.Trx!))
        {
            (membership.TryGetValue(test, out var owners) ? owners : membership[test] = []).Add(name);
        }
    }

    var missing = fullTests.Where(test => !membership.ContainsKey(test)).Order(StringComparer.Ordinal).ToList();
    var extra = membership.Keys.Where(test => !fullTests.Contains(test)).Order(StringComparer.Ordinal).ToList();
    var duplicated = membership
        .Where(entry => entry.Value.Count > 1)
        .Select(entry => $"{entry.Key} ({string.Join(", ", entry.Value)})")
        .Order(StringComparer.Ordinal)
        .ToList();
    var oversized = perShard
        .Where(entry => entry.Shard != UnitShard && entry.Outcome.Counters is { Total: > AdapterSelectLimit })
        .Select(entry => entry.Shard)
        .ToList();
    var shardTotal = perShard.Sum(entry => entry.Outcome.Counters?.Total ?? 0);
    var fullTotal = full.Counters?.Total ?? 0;

    Console.WriteLine();
    Console.WriteLine("== shard partition of the Library suite ==");

    foreach (var (name, outcome) in perShard)
    {
        Console.WriteLine($"  {name,-16}{outcome.Counters?.Total ?? 0,8:N0} tests");
    }

    Console.WriteLine($"  {"all shards",-16}{shardTotal,8:N0} tests");
    Console.WriteLine($"  {"unfiltered",-16}{fullTotal,8:N0} tests");
    PrintList("run by no shard", missing);
    PrintList("run by more than one shard", duplicated);
    PrintList("run by a shard but not by the unfiltered suite", extra);
    PrintList($"name-filtered shards selecting more than {AdapterSelectLimit:N0} tests (the adapter would drop the filter)", oversized);

    var partitioned = missing.Count == 0 && extra.Count == 0 && duplicated.Count == 0 && oversized.Count == 0 && shardTotal == fullTotal;
    var passed = full.Succeeded && perShard.All(entry => entry.Outcome.Succeeded);

    Console.WriteLine(partitioned
        ? "partition: PASS (every Library test runs in exactly one shard)"
        : "partition: FAIL (fix .github/workflows/composition-shards.json; CompositionShardTests in the Architecture suite explains the structural rules)");
    Console.WriteLine($"RESULT: {(partitioned && passed ? "PASS" : "FAIL")}{(passed ? string.Empty : " (tests failed; see above)")}");

    return partitioned && passed ? 0 : 1;
}

int ReportDurations(List<string> inputs)
{
    var files = inputs
        .SelectMany(input => Directory.Exists(input)
            ? Directory.EnumerateFiles(input, "*.trx", SearchOption.AllDirectories)
            : [input])
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (files.Count == 0)
    {
        return Fail("no trx files found");
    }

    foreach (var file in files)
    {
        if (!File.Exists(file))
        {
            return Fail($"not found: {file}");
        }

        var run = Trx.Durations(file);
        var summed = run.Tests.Sum(test => test.Seconds);
        var longest = run.Tests.MaxBy(test => test.Seconds);

        var relative = Path.GetRelativePath(Environment.CurrentDirectory, file);

        Console.WriteLine();
        Console.WriteLine($"== {(relative.StartsWith("..", StringComparison.Ordinal) ? file : relative)} ==");
        Console.WriteLine($"  wall {run.Wall.TotalSeconds,6:N0} s · summed {summed,6:N0} s · {run.Tests.Count:N0} tests · workers busy together ≈ {(summed > 0 ? summed / Math.Max(run.Wall.TotalSeconds, 1) : 0):N2}x");

        if (longest is not null)
        {
            Console.WriteLine($"  floor (longest single test) {longest.Seconds,6:N1} s  {longest.Fixture}.{longest.Name}");
        }

        Console.WriteLine("  -- fixtures by summed duration --");

        foreach (var fixture in run.Tests
            .GroupBy(test => test.Fixture, StringComparer.Ordinal)
            .Select(group => (Name: group.Key, Seconds: group.Sum(test => test.Seconds), Count: group.Count(), Max: group.Max(test => test.Seconds)))
            .OrderByDescending(fixture => fixture.Seconds)
            .Take(15))
        {
            Console.WriteLine($"  {fixture.Seconds,8:N1} s  n={fixture.Count,4}  max={fixture.Max,6:N1} s  {fixture.Name}");
        }

        Console.WriteLine("  -- longest tests --");

        foreach (var test in run.Tests.OrderByDescending(test => test.Seconds).Take(12))
        {
            Console.WriteLine($"  {test.Seconds,8:N1} s  {test.Fixture}.{test.Name}");
        }
    }

    return 0;
}

Outcome RunProject(Project project, string resultsSubdirectory, string? filter, bool noBuild)
{
    var directory = Path.Combine(resultsRoot, resultsSubdirectory);

    if (Directory.Exists(directory))
    {
        Directory.Delete(directory, recursive: true);
    }

    Directory.CreateDirectory(directory);

    var arguments = new List<string>
    {
        "test", Path.Combine(repositoryRoot, project.Path), "-c", configuration, "--logger", "trx", "--results-directory", directory,
        "--blame-hang-timeout", HangTimeout, "--blame-hang-dump-type", "none",
    };

    if (noBuild)
    {
        arguments.Add("--no-build");
    }

    if (filter is not null)
    {
        arguments.AddRange(["--filter", filter]);
    }

    var collect = coverage && project.Coverable;
    var runSettings = new List<string>();

    if (collect)
    {
        arguments.Add("--collect:XPlat Code Coverage");

        // coverlet's default per-hit counter increment is a ~4x tax on the composition search's hot loops; SingleHit
        // records each sequence point once and reports identical line and branch coverage.
        runSettings.Add("DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover");
        runSettings.Add("DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.SingleHit=true");
    }

    if (filter is not null)
    {
        // VSTest exits 0 with a warning when a filter matches nothing, which would let a shard whose fixture names
        // stopped matching pass silently.
        runSettings.Add("RunConfiguration.TreatNoTestsAsError=true");
    }

    if (runSettings.Count > 0)
    {
        arguments.Add("--");
        arguments.AddRange(runSettings);
    }

    var title = $"{project.Name}: dotnet {string.Join(' ', arguments.Select(argument => argument.Contains(' ', StringComparison.Ordinal) ? $"\"{argument}\"" : argument))}";
    Console.WriteLine(onGitHub ? $"::group::{title}" : $"== {title} ==");

    var stopwatch = Stopwatch.StartNew();
    var exitCode = RunDotnet(arguments);
    stopwatch.Stop();

    if (onGitHub)
    {
        Console.WriteLine("::endgroup::");
    }

    var trx = Directory.EnumerateFiles(directory, "*.trx", SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();

    return new Outcome(project.Name, exitCode, stopwatch.Elapsed, trx is null ? null : Trx.Counters(trx), trx);
}

int RunDotnet(IReadOnlyList<string> arguments)
{
    var startInfo = new ProcessStartInfo("dotnet") { WorkingDirectory = repositoryRoot, UseShellExecute = false };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    Console.Out.Flush();

    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("dotnet did not start");
    process.WaitForExit();

    return process.ExitCode;
}

int Summarize(List<Outcome> outcomes)
{
    Console.WriteLine();
    Console.WriteLine("== summary ==");
    Console.WriteLine($"  {"project",-16}{"tests",8}{"passed",8}{"failed",8}{"skipped",8}{"seconds",10}");

    foreach (var outcome in outcomes)
    {
        var counters = outcome.Counters;
        var cells = counters is null
            ? $"{"-",8}{"-",8}{"-",8}{"-",8}"
            : $"{counters.Total,8:N0}{counters.Passed,8:N0}{counters.Failed,8:N0}{counters.NotExecuted,8:N0}";

        Console.WriteLine($"  {outcome.Name,-16}{cells}{outcome.Elapsed.TotalSeconds,10:N1}{(outcome.Succeeded ? string.Empty : $"   FAILED (exit {outcome.ExitCode})")}");
    }

    var succeeded = outcomes.All(outcome => outcome.Succeeded);
    var total = outcomes.Sum(outcome => outcome.Counters?.Total ?? 0);
    var failed = outcomes.Sum(outcome => outcome.Counters?.Failed ?? 0);
    var seconds = outcomes.Sum(outcome => outcome.Elapsed.TotalSeconds);

    Console.WriteLine($"RESULT: {(succeeded ? "PASS" : "FAIL")} ({total:N0} tests, {failed:N0} failed, {seconds:N0} s; trx under {resultsRoot})");

    return succeeded ? 0 : 1;
}

string? ShardFilter(string name)
{
    if (name == UnitShard)
    {
        return UnitFilter;
    }

    var map = ReadShardMap();

    if (!map.TryGetValue(name, out var fixtures))
    {
        Fail($"unknown shard '{name}'; use {UnitShard} or one of: {string.Join(", ", map.Keys)}");
        return null;
    }

    if (fixtures.Length == 0)
    {
        Fail($"shard '{name}' lists no fixtures in {shardMapPath}; an empty filter would run the whole suite");
        return null;
    }

    return string.Join('|', fixtures.Select(fixture => $"FullyQualifiedName~{fixture}."));
}

Dictionary<string, string[]> ReadShardMap()
{
    using var document = JsonDocument.Parse(File.ReadAllText(shardMapPath));

    if (document.RootElement.ValueKind != JsonValueKind.Object)
    {
        throw new InvalidDataException($"{shardMapPath} must be a JSON object of shard name to fixture full names");
    }

    return document.RootElement
        .EnumerateObject()
        .ToDictionary(
            shard => shard.Name,
            shard => shard.Value.EnumerateArray().Select(fixture => fixture.GetString() ?? throw new InvalidDataException($"shard '{shard.Name}' in {shardMapPath} lists a non-string entry")).ToArray(),
            StringComparer.Ordinal);
}

void PrintList(string title, List<string> items)
{
    if (items.Count == 0)
    {
        return;
    }

    Console.WriteLine($"  {title}: {items.Count:N0}");

    foreach (var item in items.Take(25))
    {
        Console.WriteLine($"    {item}");
    }

    if (items.Count > 25)
    {
        Console.WriteLine($"    ... and {items.Count - 25:N0} more");
    }
}

int Fail(string message)
{
    Console.Error.WriteLine(onGitHub ? $"::error title=scripts/test.cs::{message}" : $"error: {message}");
    return 2;
}

static string SourcePath([CallerFilePath] string path = "") => path;

sealed record Project(string Name, string Path, bool Coverable);

sealed record Counters(int Total, int Passed, int Failed, int NotExecuted);

sealed record Outcome(string Name, int ExitCode, TimeSpan Elapsed, Counters? Counters, string? Trx)
{
    public bool Succeeded => ExitCode == 0 && Counters is { Failed: 0 };
}

sealed record TestDuration(string Fixture, string Name, double Seconds);

sealed record TestRun(TimeSpan Wall, List<TestDuration> Tests);

static class Trx
{
    private const string LibraryTestsPrefix = "BaroquenMelody.Library.Tests.";

    private static readonly XNamespace Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    public static Counters Counters(string path)
    {
        var counters = XDocument.Load(path).Descendants(Namespace + "Counters").First();

        return new Counters(Int(counters, "total"), Int(counters, "passed"), Int(counters, "failed"), Int(counters, "notExecuted"));
    }

    // Executed tests keyed by fixture and test name (with its case arguments), the identity that survives the
    // adapter's per-run test ids.
    public static HashSet<string> ExecutedTests(string path)
    {
        var document = XDocument.Load(path);
        var classById = ClassById(document);

        return document
            .Descendants(Namespace + "UnitTestResult")
            .Select(result => $"{classById[(string)result.Attribute("testId")!]}::{(string)result.Attribute("testName")!}")
            .ToHashSet(StringComparer.Ordinal);
    }

    public static TestRun Durations(string path)
    {
        var document = XDocument.Load(path);
        var classById = ClassById(document);
        var times = document.Descendants(Namespace + "Times").First();
        var wall = DateTimeOffset.Parse((string)times.Attribute("finish")!, CultureInfo.InvariantCulture) - DateTimeOffset.Parse((string)times.Attribute("start")!, CultureInfo.InvariantCulture);
        var tests = document
            .Descendants(Namespace + "UnitTestResult")
            .Select(result => new TestDuration(
                StripPrefix(classById[(string)result.Attribute("testId")!]),
                (string)result.Attribute("testName")!,
                TimeSpan.TryParse((string?)result.Attribute("duration"), CultureInfo.InvariantCulture, out var duration) ? duration.TotalSeconds : 0))
            .ToList();

        return new TestRun(wall, tests);
    }

    private static Dictionary<string, string> ClassById(XDocument document)
    {
        return document
            .Descendants(Namespace + "UnitTest")
            .ToDictionary(
                test => (string)test.Attribute("id")!,
                test => (string)test.Element(Namespace + "TestMethod")!.Attribute("className")!,
                StringComparer.Ordinal);
    }

    private static string StripPrefix(string className) => className.StartsWith(LibraryTestsPrefix, StringComparison.Ordinal) ? className[LibraryTestsPrefix.Length..] : className;

    private static int Int(XElement element, string attribute) => int.TryParse((string?)element.Attribute(attribute), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
}

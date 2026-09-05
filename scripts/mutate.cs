#!/usr/bin/env dotnet
// The full Library mutation run, sharded. .github/workflows/mutation.yml runs one job per key of
// .github/workflows/mutation-shards.json through this script (each key lists top-level folders of
// src/BaroquenMelody.Library; "." is the project's root files), then merges the shard reports into the one
// "library" report the Stryker dashboard shows. The same commands run locally.
//
//   dotnet run scripts/mutate.cs -- --shard ornamentation [stryker args...]   one shard: the map's folders become -m globs; unrecognised arguments go to dotnet stryker
//   dotnet run scripts/mutate.cs -- --merge <dir>... --output <file>          merge every mutation-report.json under the folders into one report and print its summary
//   dotnet run scripts/mutate.cs -- --summary <report>                        print the score summary of a report (markdown; CI appends it to the step summary)
//   dotnet run scripts/mutate.cs -- --upload <report> --version <name>        PUT a report to the Stryker dashboard as module "library" (needs STRYKER_DASHBOARD_API_KEY)
//
// Options: --output <dir|file> (the shard's Stryker output folder, or the merged report path) · --module <name>
// (upload; default library) · --test-project <dir> (default tests/BaroquenMelody.Library.Tests).

using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

const string LibraryProjectFolder = "BaroquenMelody.Library";
const string DefaultTestProject = "tests/BaroquenMelody.Library.Tests";
const string DefaultModule = "library";
const string DashboardBaseUrl = "https://dashboard.stryker-mutator.io/api/reports";
const string ApiKeyVariable = "STRYKER_DASHBOARD_API_KEY";
const string RootFolder = ".";
const string FilteredReason = "Removed by mutate filter";

// The statuses a mutant can only have after a test run; Ignored (any filter) and CompileError never prove
// that a shard ran the file's tests.
string[] testedStatuses = ["Killed", "Survived", "Timeout", "NoCoverage", "RuntimeError"];

const string Usage = """
    usage: dotnet run scripts/mutate.cs -- <mode> [options] [stryker arguments]

    modes
      --shard <name>            run one shard of the full Library mutation; the map's folders become -m globs and
                                every argument this script does not recognise is passed to dotnet stryker
      --merge <dir>...          merge every mutation-report.json found under the folders into one report
      --summary <report>        print a report's score summary (markdown)
      --upload <report>         PUT a report to the Stryker dashboard (reads STRYKER_DASHBOARD_API_KEY)

    options
      --output <path>           shard: Stryker's output folder (-O); merge: the merged report file
      --version <name>          upload: the dashboard version (branch, tag or sha)
      --module <name>           upload: the dashboard module (default library)
      --test-project <dir>      the test project Stryker runs from (default tests/BaroquenMelody.Library.Tests)
    """;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var repositoryRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourcePath())!, ".."));
var shardMapPath = Path.Combine(repositoryRoot, ".github", "workflows", "mutation-shards.json");
var onGitHub = string.Equals(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"), "true", StringComparison.OrdinalIgnoreCase);

string? shard = null;
var mergeInputs = new List<string>();
string? summaryReport = null;
string? uploadReport = null;
string? output = null;
string? version = null;
var module = DefaultModule;
var testProject = DefaultTestProject;
var strykerArguments = new List<string>();

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--shard":
            if ((shard = TakeValue()) is null) return Fail("--shard needs a shard name");
            break;
        case "--merge":
            while (TakeValue() is { } input) mergeInputs.Add(input);
            if (mergeInputs.Count == 0) return Fail("--merge needs at least one folder");
            break;
        case "--summary":
            if ((summaryReport = TakeValue()) is null) return Fail("--summary needs a report file");
            break;
        case "--upload":
            if ((uploadReport = TakeValue()) is null) return Fail("--upload needs a report file");
            break;
        case "--output":
            if ((output = TakeValue()) is null) return Fail("--output needs a path");
            break;
        case "--version":
            if ((version = TakeValue()) is null) return Fail("--version needs a name");
            break;
        case "--module":
            if ((module = TakeValue()!) is null) return Fail("--module needs a name");
            break;
        case "--test-project":
            if ((testProject = TakeValue()!) is null) return Fail("--test-project needs a folder");
            break;
        case "--help" or "-h" or "-?":
            Console.WriteLine(Usage);
            return 0;
        default:
            strykerArguments.Add(args[i]);
            break;
    }

    string? TakeValue()
    {
        if (i + 1 >= args.Length || args[i + 1].StartsWith('-'))
        {
            return null;
        }

        return args[++i];
    }
}

var modes = new[] { shard is not null, mergeInputs.Count > 0, summaryReport is not null, uploadReport is not null }.Count(static selected => selected);

if (modes != 1)
{
    return Fail($"choose exactly one mode{Environment.NewLine}{Usage}");
}

if (shard is not null)
{
    return RunShard(shard);
}

if (mergeInputs.Count > 0)
{
    return Merge(mergeInputs);
}

if (summaryReport is not null)
{
    return Summarize(summaryReport);
}

return await Upload(uploadReport!);

int RunShard(string name)
{
    var map = ReadShardMap();

    if (!map.TryGetValue(name, out var folders))
    {
        return Fail($"unknown shard '{name}'; use one of: {string.Join(", ", map.Keys)}");
    }

    if (folders.Length == 0)
    {
        return Fail($"shard '{name}' lists no folders in {shardMapPath}; an empty mutate filter would mutate nothing");
    }

    if (strykerArguments.Any(static argument => argument == "-m" || argument.StartsWith("-m:", StringComparison.Ordinal) || argument.StartsWith("--mutate", StringComparison.Ordinal)))
    {
        return Fail("the shard's -m globs come from the map; do not pass -m/--mutate as well");
    }

    var arguments = new List<string> { "stryker" };

    foreach (var glob in folders.Select(FolderGlob).Append("!**/obj/**").Append("!**/bin/**"))
    {
        arguments.Add("-m");
        arguments.Add(glob);
    }

    if (output is not null)
    {
        arguments.Add("-O");
        arguments.Add(Path.GetFullPath(Path.Combine(repositoryRoot, output)));
    }

    arguments.AddRange(strykerArguments);

    var workingDirectory = Path.GetFullPath(Path.Combine(repositoryRoot, testProject));
    Console.WriteLine($"shard {name}: {string.Join(", ", folders)}");
    Console.WriteLine($"{workingDirectory}> dotnet {string.Join(' ', arguments.Select(Quote))}");
    Console.Out.Flush();

    var startInfo = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory, UseShellExecute = false };

    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    var stopwatch = Stopwatch.StartNew();
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("dotnet did not start");
    process.WaitForExit();

    Console.WriteLine($"shard {name}: dotnet stryker exited {process.ExitCode} after {stopwatch.Elapsed.TotalMinutes:N1} min");

    return process.ExitCode;
}

int Merge(List<string> inputs)
{
    var reportPaths = inputs
        .Select(input => Path.GetFullPath(Path.Combine(repositoryRoot, input)))
        .SelectMany(folder => Directory.Exists(folder)
            ? Directory.EnumerateFiles(folder, "mutation-report.json", SearchOption.AllDirectories)
            : File.Exists(folder) ? [folder] : [])
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (reportPaths.Count == 0)
    {
        return Fail($"no mutation-report.json found under: {string.Join(", ", inputs)}");
    }

    // A shard's report lists every file. Files outside its globs usually have no mutants, but Stryker compiles
    // every mutant before filtering, so a file can carry its compile-error mutants in every shard, and the
    // odd file carries its whole mutant list: the mutants the mutate filter removed are marked "Removed by
    // mutate filter", while the ones an earlier filter (coverage exclusion, ignored methods) removed keep that
    // filter's reason. So: drop the filter-removed mutants, take each file from the one shard where it has
    // mutants with a test-run status (its owner), and fall back to the fullest copy for files that have only
    // ignored or compile-error mutants; a file with test-run statuses in two shards is a partition violation.
    JsonObject? merged = null;
    var candidates = new Dictionary<string, List<(string Label, JsonObject File, int Tested)>>(StringComparer.Ordinal);
    var mergedTestFiles = new JsonObject();
    var projectRoots = new HashSet<string>(StringComparer.Ordinal);

    foreach (var reportPath in reportPaths)
    {
        var report = JsonNode.Parse(File.ReadAllText(reportPath))?.AsObject() ?? throw new InvalidDataException($"{reportPath} is not a JSON object");
        var relative = Path.GetRelativePath(repositoryRoot, reportPath);
        var label = relative.StartsWith("..", StringComparison.Ordinal) ? reportPath : relative;
        var mutated = 0;

        projectRoots.Add(report["projectRoot"]?.GetValue<string>() ?? string.Empty);
        merged ??= new JsonObject(report.Where(static property => property.Key is not "files" and not "testFiles").Select(property => KeyValuePair.Create(property.Key, property.Value?.DeepClone())));

        foreach (var (path, file) in report["files"]?.AsObject() ?? [])
        {
            var copy = file?.DeepClone().AsObject() ?? throw new InvalidDataException($"{label}: {path} is not a JSON object");
            var mutants = copy["mutants"]?.AsArray() ?? throw new InvalidDataException($"{label}: {path} has no mutants array");

            for (var index = mutants.Count - 1; index >= 0; index--)
            {
                if (mutants[index]?["statusReason"]?.GetValue<string>() == FilteredReason)
                {
                    mutants.RemoveAt(index);
                }
            }

            var tested = mutants.Count(mutant => testedStatuses.Contains(mutant?["status"]?.GetValue<string>() ?? string.Empty, StringComparer.Ordinal));

            if (tested > 0)
            {
                mutated++;
            }

            (candidates.TryGetValue(path, out var list) ? list : candidates[path] = []).Add((label, copy, tested));
        }

        foreach (var (path, testFile) in report["testFiles"]?.AsObject() ?? [])
        {
            mergedTestFiles[path] ??= testFile?.DeepClone();
        }

        Console.Error.WriteLine($"merged {label}: {mutated} mutated files");
    }

    var mergedFiles = new JsonObject();
    var duplicated = new List<string>();

    foreach (var (path, entries) in candidates.OrderBy(static entry => entry.Key, StringComparer.Ordinal))
    {
        var owners = entries.Where(static entry => entry.Tested > 0).ToList();

        if (owners.Count > 1)
        {
            duplicated.Add($"{path} ({string.Join(", ", owners.Select(static owner => owner.Label))})");
        }

        var chosen = owners.Count > 0
            ? owners.MaxBy(static owner => owner.Tested)
            : entries.MaxBy(static entry => entry.File["mutants"]?.AsArray().Count ?? 0);

        mergedFiles[path] = chosen.File;
    }

    if (duplicated.Count > 0)
    {
        // Nothing is written: a double-counted report must never reach the artifact or the dashboard.
        Console.Error.WriteLine($"error: {duplicated.Count} files have test-run results in more than one shard (the shard map must partition the Library); no report written:");

        foreach (var entry in duplicated.Take(25))
        {
            Console.Error.WriteLine($"  {entry}");
        }

        return 1;
    }

    merged!["files"] = mergedFiles;
    merged["testFiles"] = mergedTestFiles;

    if (projectRoots.Count > 1)
    {
        Console.Error.WriteLine($"warning: the reports name different project roots ({string.Join(", ", projectRoots)}); file paths may not line up");
    }

    var outputPath = Path.GetFullPath(Path.Combine(repositoryRoot, output ?? Path.Combine("StrykerOutput", DefaultModule, "reports", "mutation-report.json")));
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    var json = merged.ToJsonString();
    File.WriteAllText(outputPath, json);
    Console.Error.WriteLine($"wrote {outputPath}");
    WriteHtml(reportPaths, json, Path.ChangeExtension(outputPath, ".html"));

    PrintSummary(merged, $"{reportPaths.Count} shard reports");

    return 0;
}

int Summarize(string reportFile)
{
    var reportPath = Path.GetFullPath(Path.Combine(repositoryRoot, reportFile));

    if (!File.Exists(reportPath))
    {
        return Fail($"not found: {reportPath}");
    }

    var report = JsonNode.Parse(File.ReadAllText(reportPath))?.AsObject() ?? throw new InvalidDataException($"{reportPath} is not a JSON object");
    PrintSummary(report, Path.GetRelativePath(repositoryRoot, reportPath));

    return 0;
}

async Task<int> Upload(string reportFile)
{
    var reportPath = Path.GetFullPath(Path.Combine(repositoryRoot, reportFile));

    if (!File.Exists(reportPath))
    {
        return Fail($"not found: {reportPath}");
    }

    if (version is null)
    {
        return Fail("--upload needs --version <name> (the branch, tag or sha the dashboard files the report under)");
    }

    var apiKey = Environment.GetEnvironmentVariable(ApiKeyVariable);

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return Fail($"{ApiKeyVariable} is not set; the dashboard upload needs it");
    }

    var project = ProjectName();
    var url = $"{DashboardBaseUrl}/{project}/{version}?module={Uri.EscapeDataString(module)}";

    using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    using var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = new ByteArrayContent(await File.ReadAllBytesAsync(reportPath)) };
    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    request.Headers.Add("X-Api-Key", apiKey);

    Console.WriteLine($"PUT {url} ({new FileInfo(reportPath).Length / 1024:N0} KiB)");

    using var response = await client.SendAsync(request);
    var body = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"{(int)response.StatusCode} {response.ReasonPhrase}: {body[..Math.Min(body.Length, 500)]}");

    return response.IsSuccessStatusCode ? 0 : 1;
}

void PrintSummary(JsonObject report, string title)
{
    var projectRoot = (report["projectRoot"]?.GetValue<string>() ?? string.Empty).Replace('\\', '/').TrimEnd('/');
    var rows = new Dictionary<string, Tally>(StringComparer.Ordinal);
    var total = new Tally();

    foreach (var (path, file) in report["files"]?.AsObject() ?? [])
    {
        var relative = path.Replace('\\', '/');

        if (projectRoot.Length > 0 && relative.StartsWith(projectRoot + "/", StringComparison.Ordinal))
        {
            relative = relative[(projectRoot.Length + 1)..];
        }

        var folder = relative.Contains('/', StringComparison.Ordinal) ? relative[..relative.IndexOf('/', StringComparison.Ordinal)] : RootFolder;
        var tally = rows.TryGetValue(folder, out var existing) ? existing : rows[folder] = new Tally();

        foreach (var mutant in file?["mutants"]?.AsArray() ?? [])
        {
            var status = mutant?["status"]?.GetValue<string>() ?? "Pending";
            tally.Add(status);
            total.Add(status);
        }
    }

    Console.WriteLine($"## 🧬 {module}: {total.ScoreText} ({title})");
    Console.WriteLine();
    Console.WriteLine("| Folder | Score | Killed | Survived | Timeout | No coverage | Ignored | Compile errors | Runtime errors | Total |");
    Console.WriteLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");

    foreach (var (folder, tally) in rows.OrderBy(static entry => entry.Key == RootFolder ? 0 : 1).ThenBy(static entry => entry.Key, StringComparer.Ordinal))
    {
        Console.WriteLine(tally.Row(folder));
    }

    Console.WriteLine(total.Row("**Total**"));
    Console.WriteLine();
    Console.WriteLine($"RESULT: mutation score {total.ScoreText}, {total.Detected:N0} detected, {total.Undetected:N0} undetected, {total.Total:N0} mutants");
}

// Stryker's html report is a static shell with the json assigned to the report element on one line
// (`app.report = {...};`), so the merged report gets the same shell from one of the shards with that line replaced.
void WriteHtml(List<string> reportPaths, string json, string htmlPath)
{
    const string assignment = "app.report = ";
    var template = reportPaths.Select(static path => Path.ChangeExtension(path, ".html")).FirstOrDefault(File.Exists);

    if (template is null)
    {
        Console.Error.WriteLine("no mutation-report.html next to a shard report; skipping the merged html");
        return;
    }

    var lines = File.ReadAllLines(template);
    var index = Array.FindIndex(lines, static line => line.TrimStart().StartsWith(assignment, StringComparison.Ordinal));

    if (index < 0)
    {
        Console.Error.WriteLine($"{template} has no `{assignment}` line; skipping the merged html");
        return;
    }

    var indent = lines[index][..(lines[index].Length - lines[index].TrimStart().Length)];
    lines[index] = $"{indent}{assignment}{json};";
    File.WriteAllLines(htmlPath, lines);
    Console.Error.WriteLine($"wrote {htmlPath}");
}

string FolderGlob(string folder) => folder == RootFolder ? $"**/{LibraryProjectFolder}/*.cs" : $"**/{LibraryProjectFolder}/{folder}/**/*.cs";

string ProjectName()
{
    var configPath = Path.GetFullPath(Path.Combine(repositoryRoot, testProject, "stryker-config.json"));
    using var config = JsonDocument.Parse(File.ReadAllText(configPath));

    return config.RootElement.GetProperty("stryker-config").GetProperty("project-info").GetProperty("name").GetString()
        ?? throw new InvalidDataException($"{configPath} has no stryker-config.project-info.name");
}

Dictionary<string, string[]> ReadShardMap()
{
    using var document = JsonDocument.Parse(File.ReadAllText(shardMapPath));

    if (document.RootElement.ValueKind != JsonValueKind.Object)
    {
        throw new InvalidDataException($"{shardMapPath} must be a JSON object of shard name to Library folder names");
    }

    return document.RootElement
        .EnumerateObject()
        .ToDictionary(
            entry => entry.Name,
            entry => entry.Value.EnumerateArray().Select(folder => folder.GetString() ?? throw new InvalidDataException($"shard '{entry.Name}' in {shardMapPath} lists a non-string entry")).ToArray(),
            StringComparer.Ordinal);
}

int Fail(string message)
{
    Console.Error.WriteLine(onGitHub ? $"::error title=scripts/mutate.cs::{message}" : $"error: {message}");
    return 2;
}

static string Quote(string argument) => argument.Contains(' ', StringComparison.Ordinal) ? $"\"{argument}\"" : argument;

static string SourcePath([CallerFilePath] string path = "") => path;

sealed class Tally
{
    private readonly Dictionary<string, int> counts = new(StringComparer.Ordinal);

    public int Total => counts.Values.Sum();

    public int Detected => Count("Killed") + Count("Timeout");

    public int Undetected => Count("Survived") + Count("NoCoverage");

    public string ScoreText => Detected + Undetected == 0 ? "n/a" : $"{100.0 * Detected / (Detected + Undetected):N2} %";

    public void Add(string status) => counts[status] = Count(status) + 1;

    public string Row(string label) =>
        $"| {label} | {ScoreText} | {Count("Killed"):N0} | {Count("Survived"):N0} | {Count("Timeout"):N0} | {Count("NoCoverage"):N0} | {Count("Ignored"):N0} | {Count("CompileError"):N0} | {Count("RuntimeError"):N0} | {Total:N0} |";

    private int Count(string status) => counts.TryGetValue(status, out var count) ? count : 0;
}

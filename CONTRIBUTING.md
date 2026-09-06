# How to Contribute

Thanks for your interest in contributing to `baroquen-melody`! Here are a few general guidelines on contributing and
reporting bugs that we ask you to review. Following these guidelines helps to communicate that you respect the time of
the contributors managing and developing this open source project. In return, they should reciprocate that respect in
addressing your issue, assessing changes, and helping you finalize your pull requests. In that spirit of mutual respect,
we endeavour to review incoming issues and pull requests within 10 days, and will close any lingering issues or pull
requests after 60 days of inactivity.

Please note that all of your interactions in the project are subject to our [Code of Conduct](CODE_OF_CONDUCT.md). This
includes creation of issues or pull requests, commenting on issues or pull requests, and extends to all interactions in
any real-time space (eg. Slack, Discord, etc).

## Reporting Issues

Before reporting a new issue, please ensure that the issue was not already reported or fixed by searching through our
[issues list](https://github.com/wbaldoumas/baroquen-melody/issues).

When creating a new issue, please be sure to include a **title and clear description**, as much relevant information as
possible, and, if possible, a test case.

**If you discover a security bug, please do not report it through GitHub. Instead, please see security procedures in
[SECURITY.md](SECURITY.md).**

## Sending Pull Requests

Before sending a new pull request, take a look at existing pull requests and issues to see if the proposed change or fix
has been discussed in the past, or if the change was already implemented but not yet released.

We expect new pull requests to include tests for any affected behavior, to keep the analyzers and the architecture
tests green, and, as we follow semantic versioning, we may reserve breaking changes until the next major version
release.

### Building and Testing

The repository targets the .NET SDK pinned in `global.json` (`dotnet --version` must report at least that feature
band; Renovate moves the pin, so update your SDK when it does). The solution also contains the .NET MAUI host, which
needs the MAUI workloads — you do not need them to work on the composition engine, the infrastructure or the Blazor
components. `scripts/test.cs` (a .NET 10 file-based app; nothing to install beyond the SDK) runs the four test
projects the way CI does, and `dotnet test` on a single project is still the quickest focused check:

```bash
dotnet run scripts/test.cs                               # all four suites in Release; about 75 seconds on a 16-core machine
dotnet run scripts/test.cs -- --shard composition-b      # one CI shard, exactly as its matrix job runs it
dotnet run scripts/test.cs -- --verify-shards            # after editing the shard map: the shards must still partition the Library suite
dotnet test tests/BaroquenMelody.Architecture.Tests/     # architecture rules and the shard-map guard, about ten seconds
```

Every project treats analyzer warnings (StyleCop, Meziantou, the .NET analyzers) as errors, so a clean build is the
first gate. CI (`.github/workflows/test.yml`) runs the same script on every pull request and fails on any red test: the
Library suite's seeded composition sweeps (fixtures tagged `[Category("Composition")]`) are split across matrix jobs by
`.github/workflows/composition-shards.json`, and everything else runs in the `unit` job. A new sweep fixture goes into
one shard in the same pull request; the Architecture suite fails when one is missing from the map. Every run is
bounded: ten minutes without any test starting or finishing terminates the test host, and a `Sequence_*.xml` inside
the run's `test-results-*` artifact lists the tests that were in flight.

The `test` job, which waits for every shard, and the `lint` job are the two checks `.github/rulesets/main.json`
requires of a pull request into `main` (pull requests only, no force-pushes or deletion; administrators may bypass
from the merge dialog). The ruleset is applied by hand, from the repository's Rules settings or with
`gh api --method POST repos/{owner}/{repo}/rulesets --input .github/rulesets/main.json`.

### Architecture Tests

`tests/BaroquenMelody.Architecture.Tests` uses [ArchUnitNET](https://github.com/TNG/ArchUnitNET) to keep the
project's structure honest: the dependency direction between assemblies (Infrastructure ← Library ← UI, console app
and benchmarks), where types live (enums in `*.Enums`, composition rules in `Rules.*`, components in `Layout`, `Pages`
or `Shared`), type shapes (sealed classes, record configurations, the Fluxor store's state/action/reducer/effect
shapes), forbidden dependencies (`System.Random` outside the random providers, `System.Console` outside the console
app, static `System.IO` inside the library, the policy engine outside the ornamentation and dynamics engines) and
test-suite conventions (`internal sealed class *Tests`, no `[Explicit]` or `[Ignore]`, new fixtures building their
configurations through `TestCompositionConfigurations`).

If one of these tests fails on your branch, its message names the rule, states why the rule exists and lists the
offending types. Please fix the code rather than the rule — every rule describes how the codebase is already written.
If you believe a convention should change, say so in your pull request and change the rule in the same PR, updating
its `Because()` so the reason travels with it. One rule is frozen against a committed baseline of pre-existing
violations (`FrozenViolations/`): if you fix or rename one of those fixtures, rerun the tests and commit the
regenerated JSON rather than editing it by hand.

### Mutation Testing

Pull requests are mutation-tested with [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/): Stryker plants small
bugs ("mutants") in the code your change touches and checks that the tests catch them. The `Mutation` workflow posts a
per-project summary to the run's job summary and uploads the full HTML reports as artifacts. The pull-request legs are
advisory: Stryker gets a twelve-minute budget, and a change that re-enables most of the Library's mutants (a broadly
covering test, a test-data helper) reports that it ran out of time, leaving the check green, instead of holding the
pull request; the full run on main covers those mutants after the merge. (A genuine Stryker failure still shows red,
though the check is not one a merge requires.) Every merge to main that
touches `src`, `tests` or the mutation tooling (and `gh workflow run mutation.yml`, or the Actions tab) runs every
project whole and refreshes the Stryker dashboard. The full Library run (2,424 testable mutants, hours in one job) is
one job per key of
`.github/workflows/mutation-shards.json` — each key lists top-level folders of `src/BaroquenMelody.Library` — with the
shard reports merged into one `library` report for the job summary, the artifact and the Stryker dashboard. A new
top-level Library folder goes into one shard in the same pull request; the Architecture suite fails when one is
missing.

To run it locally, restore the repository's tools once, then run Stryker from the test project whose source you changed
(each test project carries its own `stryker-config.json`):

```bash
dotnet tool restore

cd tests/BaroquenMelody.Library.Tests
dotnet stryker --since:main        # only mutants in code changed since main
dotnet stryker --open-report       # everything, and open the HTML report when done
```

One Library shard runs through the script CI uses, from the repository root; anything after the shard name goes to
`dotnet stryker`:

```bash
dotnet run scripts/mutate.cs -- --shard ornamentation
```

`--since` reads the git diff itself, so run it from a normal clone rather than a linked `git worktree` (there it resolves
the main checkout and reports nothing changed). It re-tests every mutant covered by a test you changed — and would
re-test *everything* when a non-C# file under a test project changes, so each `stryker-config.json` lists the test
`.csproj` and the config itself under `since.ignore-changes-in`; add any new non-C# test-project file there too.

The Library configuration excludes two NUnit categories, both declared as constants in
`tests/BaroquenMelody.Library.Tests/TestCategories.cs`: the seeded composition sweeps (fixtures tagged
`TestCategories.Composition`) take most of the suite's wall time and are left to `dotnet test`, so mutants that only
those sweeps would catch are reported as "no coverage" rather than survived; and the tests that compose a whole piece
to pin one property (tagged `TestCategories.WholeComposition`, at the method unless every test in the fixture composes)
still run in CI's unit leg but not under Stryker, because each covers nearly every mutant and they were most of a
mutant's test time while catching almost nothing the fast unit tests miss. Tag any new `Enumerable.Range(1, N)`
composition sweep `Composition` and any new whole-piece test `WholeComposition`; the Architecture suite requires the
suite to use exactly the declared names and the Stryker filter to exclude exactly them. (The filter has to be a
category filter: NUnit's adapter silently runs every test when a name-based filter selects more than 2,000 of them.)

## Other Ways to Contribute

We welcome anyone that wants to contribute to `baroquen-melody` to triage and reply to open issues to help troubleshoot
and fix existing bugs. Here is what you can do:

- Help ensure that existing issues follows the recommendations from the _[Reporting Issues](#reporting-issues)_ section,
  providing feedback to the issue's author on what might be missing.
- Review and update the existing content of our [Wiki](https://github.com/wbaldoumas/baroquen-melody/wiki) with up-to-date
  instructions and code samples.
- Review existing pull requests, and testing patches against real existing applications that use `baroquen-melody`.
- Write a test, or add a missing test case to an existing test.

Thanks again for your interest on contributing to `baroquen-melody`!

:heart:

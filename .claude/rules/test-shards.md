---
paths:
  - ".github/workflows/**"
  - "scripts/**"
  - "codecov.yml"
---

# The CI test shards

This loads when you work on the test workflow, the shard map, `scripts/test.cs` or `codecov.yml`. `.github/workflows/test.yml` runs the Library suite as one `unit` job plus one job per key of `composition-shards.json`, every job through `scripts/test.cs`, so the filter, configuration, coverage and results mechanics live in one place: change them in the script, never in the YAML, and run the same script locally (`dotnet run scripts/test.cs -- --help`).

## The contract

- `unit` = `TestCategory!=Composition` on the Library suite plus the Infrastructure, App.Components and Architecture projects. It is a category clause because the NUnit adapter always honours those; any other filter that selects more than 2,000 tests is dropped silently and the whole suite runs.
- A composition shard = one `FullyQualifiedName~<fixture full name>.` clause per listed fixture, joined with `|`. The trailing dot stops a fixture matching a longer name. `CompositionShardTests` (Architecture suite, ~10 s) rejects fixtures a name filter cannot select (parameterized, sourced, generic or nested) and method-level `Composition` tags, and requires every tagged fixture in exactly one shard, every shard key in the matrix, no `unit` key, and `codecov.yml`'s `after_n_builds` (both occurrences) equal to the matrix size: Codecov counts one upload per job, so fewer posts statuses on partial coverage and more never posts.
- Every shard run passes `RunConfiguration.TreatNoTestsAsError=true` because VSTest exits 0 with a warning when a filter matches nothing; the script also refuses a shard whose list is empty.
- `dotnet run scripts/test.cs -- --verify-shards` holds the behaviour the guard cannot: it runs the unfiltered Library suite and every shard, then requires the union of executed tests to equal the full set with no test in two shards and no name-filtered shard above the adapter's 2,000-test limit (`--list-tests` cannot stand in: the adapter applies filters at execution). Run it after any change to the map, the filter derivation, or a sweep fixture's shape; it takes a few minutes.

## Balancing

- Balance by runner time only: the private-repository runner is 2 vCPU with 2 NUnit workers and ~1.5x slower per core than a developer machine, so local durations mislead. Download a recent run's `test-results-*` artifacts (`gh run download <run id> --dir <dir>`) and read them with `dotnet run scripts/test.cs -- --durations <dir>`: summed seconds per fixture is a shard's load, the longest single test is its floor (a 45–55 s sweep cannot be split), and the runner varies by about ±45 s on an identical shard between runs, so stop balancing inside that tolerance.
- Each shard pays ~50 s of restore and build before its first test; a new shard is worth it only when the heaviest shard's test time exceeds that by a clear margin. Growing or shrinking the matrix means the same change to the matrix list and to both `after_n_builds` values in `codecov.yml`; the guard fails otherwise.
- A new `[Category("Composition")]` fixture goes into the lightest shard in the same PR; then run `--verify-shards` and read the shard's job time on the PR.

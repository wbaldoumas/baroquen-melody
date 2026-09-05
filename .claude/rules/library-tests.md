---
paths:
  - "tests/BaroquenMelody.Library.Tests/**"
---

# Writing seeded tests in BaroquenMelody.Library.Tests

This loads only when you work inside `tests/BaroquenMelody.Library.Tests`. The pipeline-level determinism facts (one shared seeded stream, the second stream for the processor shuffle, insertion-ordered draw collections, draw-aligned A/B controls) stay in CLAUDE.md ("Determinism in Seeded Tests"); this file is about writing the tests.

## Configuration and seeds

- Build every `CompositionConfiguration` through `TestCompositionConfigurations.Get` (in `TestData`), never the primary constructor; a frozen architecture rule lets the 17 legacy fixtures stand and fails any new fixture type that constructs one directly (`with { … }` clones of an existing configuration are fine).
- Seeded walks differ across operating systems: assert seed-sweep existence properties (`Enumerable.Range(1, N).Any(...)`), never per-seed outcome pins; per-seed pins are only safe for properties that hold for every seed.
- `SeededComposition.Compose(configuration, seed)` gives both seeded streams; `SeededRandomProviders.ForProcessorShuffle` salts the shuffle stream so it never replays the composition's.
- Set `ShuffleOrnamentationProcessors = false` only when the test needs the configured processor order itself or compares against a pre-seeded-shuffle baseline; shuffle-on vs shuffle-off is not a draw-aligned pair.

## Comparing note lists

- Compare `SeededComposition.Notes` lists with the ordered `Should().Equal(...)` / `Should().NotEqual(...)` only, never `BeEquivalentTo` / `NotBeEquivalentTo`. The snapshots are already in `GetNotes()` time order, so ordered inequality is the exact negation of the byte-identity check. FluentAssertions' unordered structural matching is quadratic: measured on 2026-09-04 at ~23 s per ~1,000-note list against 1.25 s for the two compositions being compared, and four such assertions were 59 % of the suite's CPU time.
- `SeededDeterminismTests` guards process-history independence; a new determinism claim belongs there, next to the existing byte-identity pairs.

## Parallel execution

- Fixtures run in parallel (`[assembly: Parallelizable(ParallelScope.Fixtures)]` in `AssemblyInfo.cs`); tests inside a fixture stay sequential and the worker count follows `Environment.ProcessorCount` (2 on the CI runner, 16 locally). Keep fixtures free of shared mutable state — no static caches, no `[OneTimeSetUp]` that writes to statics — or mark the fixture `[NonParallelizable]`.
- CsCheck's `Gen.Sample` already runs its iterations on every logical CPU by default; under NUnit parallelism the two multiply. Pass `threads:` (or set `CsCheck_Threads`) only if oversubscription shows in the trx artifact CI uploads.
- Independent test cases in a sweep fixture may opt into `[Parallelizable(ParallelScope.Children)]` so its cases spread across workers; existence sweeps (`.Any(...)` over seeds) stay one test.

## Cost

- Every seeded composition sweep is CI time: the 13 `[Category("Composition")]` fixtures are ~94 % of the suite. Keep `N` small, keep the fixture tagged `Composition` (Stryker's `test-case-filter` excludes the category; the NUnit adapter honours category filters and silently drops any other filter selecting > 2,000 tests), and read the per-test durations from the `test-results` artifact before adding a sweep to a fixture that is already the longest.
- CI builds Release and collects coverage with coverlet `SingleHit=true` (see the comments in `.github/workflows/test.yml`); a plain `dotnet test tests/BaroquenMelody.Library.Tests/ -c Release` is the fast local check (~80 s).

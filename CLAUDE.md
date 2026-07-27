# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Baroquen Melody is an experimental .NET 10 application that programmatically generates music in a Baroque style. It produces MIDI files by composing chord progressions, applying ornamentations, phrasing, dynamics, and musical rules.

The app runs as a .NET MAUI Blazor Hybrid application (Windows, Android, iOS, macOS) with a MudBlazor UI. There is also a standalone console project (`src/BaroquenMelody`) used for headless composition testing.

## Build and Test Commands

```bash
# Build the solution
dotnet build src/BaroquenMelody.sln

# Run all tests
dotnet test src/BaroquenMelody.sln

# Run a single test project
dotnet test tests/BaroquenMelody.Library.Tests/
dotnet test tests/BaroquenMelody.Infrastructure.Tests/
dotnet test tests/BaroquenMelody.App.Components.Tests/

# Run a single test by name
dotnet test tests/BaroquenMelody.Library.Tests/ --filter "FullyQualifiedName~ComposerTests"

# Run a test project sequentially (disables NUnit parallelization - for timing comparisons or debugging)
dotnet test tests/BaroquenMelody.Library.Tests/ -- NUnit.NumberOfTestWorkers=0

# Run benchmarks
dotnet run --project benchmarks/BaroquenMelody.Benchmarks/ -c Release

# Run the console app (headless composition)
dotnet run --project src/BaroquenMelody/
```

## Architecture

### Project Dependency Graph

```text
BaroquenMelody.App (MAUI host)
├── BaroquenMelody.App.Components (Blazor/MudBlazor UI)
│   └── BaroquenMelody.Library
│       └── BaroquenMelody.Infrastructure
└── BaroquenMelody.Library

BaroquenMelody (console app)
└── BaroquenMelody.Library
```

### Core Composition Pipeline

`BaroquenMelodyComposerConfigurator` is the central factory that wires up all composition components from a `CompositionConfiguration`. It produces an `IMidiFileComposer`.

The `Composer.Compose()` pipeline runs these steps in order:

1. **Theme** — `ThemeComposer` generates an initial thematic exposition
2. **Body** — `ChordComposer` builds chord-by-chord using `CompositionStrategy`, which uses look-ahead search with `ICompositionRule` validation
3. **Ornamentation** — `CompositionDecorator` applies baroque ornaments (turns, mordents, passing tones, runs, etc.) via a policy engine
4. **Phrasing** — `CompositionPhraser` inserts thematic repetitions
5. **Ending** — `EndingComposer` composes a cadential ending
6. **Suspensions** — `SuspensionApplicator` ties preparations across strong-beat harmonic changes and delays their resolutions (a pure time-shift; no new pitches)
7. **Tonicization** — `TonicizationApplicator` raises the thirds of minor triads approaching a chord a fifth below into true dominants (licenses derived per mode; the gate is lifted for Ionian and Aeolian), respelling every voice's figures with the raise
8. **Sustain** — Repeated notes are extended
9. **Completion** — the theme exposition is prepended, taking its own suspension and tonicization passes over the seam
10. **Dynamics** — `DynamicsApplicator` assigns velocity curves
11. **MIDI Generation** — `MidiGenerator` converts the `Composition` to a `MidiFile` (via Melanchall.DryWetMidi)

Passes share one seeded RNG stream and generally draw once per candidate or site regardless of outcome; anything that changes draw counts shifts every later pass's draws.

### Key Abstractions

- **`CompositionStrategy`** — Uses `IChordChoiceRepository` to enumerate possible next chords, validates them against `ICompositionRule`, and does a look-ahead search to ensure the composition doesn't paint itself into a corner. The chord-choice index space is defined by `NoteChoiceGenerator`'s canonical order (pinned by a deciding test), which the choice repositories and `ForwardCheckingChordChoiceEnumerator` must share exactly.
- **`ICompositionRule`** — Interface for rules like `AvoidParallelIntervals`, `AvoidDissonance`, `FollowsStandardProgression`. Combined via `AggregateCompositionRule`.
- **`ICompositionDecorator`** — Applies ornamentations using an engine built with `Atrea.PolicyEngine`. Each ornamentation type (mordent, turn, passing tone, etc.) has its own processor with input/output policies.
- **`CompositionConfiguration`** — Central config record holding tonic, mode, meter, tempo, instrument ranges, rule weights, and ornamentation settings.

### State Management

Uses **Fluxor** (Redux-like) for state management. States live in `Library/Store/State/` (e.g., `CompositionProgressState`, `InstrumentConfigurationState`). Actions and reducers follow standard Fluxor patterns. The `IDispatcher` is injected into composers to report progress.

### UI Layer

`BaroquenMelody.App.Components` is a Razor Class Library with MudBlazor components. `BaroquenMelody.App` is the MAUI host. The UI uses `Fluxor.Blazor.Web` for state binding.

## Code Conventions

- **Target framework**: `net10.0` (all projects). MAUI app additionally targets platform-specific TFMs.
- **Nullable reference types**: Enabled everywhere. `TreatWarningsAsErrors` is on.
- **Analyzers**: StyleCop, Meziantou.Analyzer, and .NET analyzers are enforced across all projects. Build will fail on analyzer warnings.
- **File-scoped namespaces**: Used throughout (`namespace Foo;`).
- **Primary constructors**: Used extensively for DI injection.
- **Test framework**: NUnit with FluentAssertions and NSubstitute for mocking. UI components are tested with bUnit (`tests/BaroquenMelody.App.Components.Tests`).
- **Internal by default**: Library/Infrastructure types are `internal` with `InternalsVisibleTo` for test and benchmark projects.
- **`PublishAot`**: Enabled on `Library`, `Infrastructure`, and the console app. Avoid reflection-heavy patterns in these projects.

## Test Parallelization

- All three test assemblies declare `[assembly: Parallelizable(ParallelScope.Fixtures)]`: fixtures run concurrently, but tests within a fixture stay sequential, so per-fixture instance state guarded by `[SetUp]` remains safe. Never share mutable static state across fixtures.
- Stateless fixtures with expensive full-composition tests may opt into `[Parallelizable(ParallelScope.All)]` at the class level. A fixture with instance fields must also take `[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]` before test-level parallelism, because NUnit otherwise shares one fixture instance across concurrent test cases.

## CI

- `test.yml` runs the three test projects as a parallel job matrix; the status checks are named `test (<ProjectName>)`. Each leg uploads its own coverage file to codecov, which merges uploads per commit.
- `codecov.yml` sets `after_n_builds: 3`, so codecov statuses and the PR comment appear only after all three legs have uploaded (the Library leg is by far the slowest); partial-coverage numbers are never reported.

## Determinism in Seeded Tests

- `ShuffleOrnamentationProcessors` defaults to `true` and is deliberately **not** seed-reproducible; seeded or comparative tests must set it to `false`.
- Composition decisions must never consume hash-layout enumeration order (`FrozenSet`/`FrozenDictionary` iteration over record or class keys): frozen layouts are not process-stable, which once made seeded output depend on what had executed earlier in the process. Canonicalize with an explicit `OrderBy` at the consumption site, or generate the canonical order directly (see `NoteChoiceGenerator`).
- Pipeline passes iterate voices via `CompositionConfiguration.Instruments` (register order, tie-broken by instrument), never the raw `InstrumentConfigurations` set; rule, scoring, and ornamentation factories order their configurations by their enum before building.
- Seeded walks differ across operating systems: assert seed-sweep existence properties (`Enumerable.Range(1, N).Any(...)`), never per-seed outcome pins; per-seed pins are only safe for properties that hold for every seed.
- A/B comparisons between two seeded runs must be draw-aligned: disabling a feature outright (`Enabled: false`) removes its RNG draws and shifts every later pass's decisions, so compare against a control that consumes identical draws (e.g. the feature enabled at `Probability: 0`) or compare only divergence-robust properties.

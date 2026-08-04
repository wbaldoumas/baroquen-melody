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
2. **Body** — `ChordComposer` builds chord-by-chord using `CompositionStrategy`, which uses look-ahead search with `ICompositionRule` validation. `HarmonicRhythmScheduler` (default on) makes phrase-interior measures hold each harmony across two beats (plain duplicate chords). `VoiceRhythmScheduler` (default on; `VoiceRhythmConfiguration`) assigns per-voice rhythm roles over `MinPhraseLength`-measure blocks of the body: the HELD voice moves once per measure — its note is pinned at the one fresh interior beat via a filter over the free candidate set (`ChordComposer.Compose(chords, pinnedNote)`; empty filter ⇒ free fallback, so holds can never dead-end a walk that would otherwise compose) — and the FLORID voice attracts the sixteenth-tier figures. The walk records emitted note instances in `VoiceRhythmLedger` (reference-keyed; deep copies carry no role), which role-aware probability gates (`RoleAwareWantsToOrnament`, substituted for `WantsToOrnament` at engine build only when enabled) read during decoration: held notes are ornament-silenced so the sustain pass ties them pair-wise into half notes, florid notes boost the subdividing tier. Held roles require the harmonic-rhythm grid and ≥3 voices; the exposition, ending, phraser copies, and ground form are never recorded and thus untouched. Disabled ⇒ behavior and draw stream are bit-identical to pre-feature (the engines carry the same policy instances; the scheduler and ledger sit inert).
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

### Composition Forms

`CompositionConfiguration.GroundBassConfiguration` (default disabled; surfaced as the Form select in the UI) makes the configurator swap the fugal `Composer` for `GroundBassComposer` (`Library/Forms/` holds the pattern bank and planner). The ground form: a bass pattern (scale-step offsets from a tonic anchor, rendered into the lowest voice's register) announces itself alone, then repeats under upper voices searched fresh at every ground-note onset with the bass pinned, threading each onset to the next pin exactly as fugal entries thread. Held slots are plain duplicates (the harmonic-rhythm idiom); dead-ended walks retry from fresh draws, with a per-site unpinned liberty on the final attempt; an unplannable bass range falls back to the fugue. The suspension and tonicization passes run over a trailing sub-composition (sharing chord references) so the solo announcement stays exact. `GroundBassFeasibilityAnalyzer` (public) reports which bank patterns fit a configuration — the planner draws from exactly that set in bank order, and the UI uses the same scan for its pattern-dropdown markers, feasibility chip, and fugue-fallback toasts (the scan draws nothing, so re-running it never perturbs seeded compositions). `GroundBassConfiguration.Pattern` pins a specific ground (`null` = one seeded draw among the fits; a pinned pattern that doesn't fit yields no plan, falling back to the fugue like an empty bank); the UI's Randomize rolls the pattern only among fits for the rolled key.

**Modulation (tonal plan)**: `GroundBassPlan.Sections` partitions statements into `TonalSection`s. `GroundBassConfiguration.Modulate` (default on; the Modulate switch in the UI) lets the planner send a formula-placed middle block of statements to the RELATIVE key (Ionian ↔ Aeolian only — the relative pair shares one pitch set, so cross-scale note resolution can never miss; other modes and non-relative targets are out until candidate generation learns out-of-scale context notes). The journey is derived draw-free and declines to home-only unless: the mode is lifted, ≥4 statements are planned (solo + accompanied home lead, ≥1 foreign, ≥1 home tail), the pattern renders feasibly in the relative scale, and both seam bass motions are a step or consonant (a pinned bass cannot dodge a dissonant seam leap — `AvoidDissonantLeaps` would starve the onset). The configurator builds a second key-bound component stack (`GroundBassSectionComponents`: strategy, seam strategy, selector, decorator, tonicization) from a freshly constructed relative configuration (never `with`-cloned — `Scale` is a property initializer); the home stack aliases the fugal singletons, so home-only plans compose byte-identically to pre-modulation. Cross-key transitions validate under the ARRIVING section's seam strategy (full rules minus `FollowStandardChordProgression` — progression grammar is intra-key; every voice-leading rule holds at seams). Decoration and tonicization run per-section slices (sharing measure refs; one section ⇒ one slice ⇒ the pre-modulation pass shape); suspensions stay whole-trailing (diatonic-step eligibility is identical across relative keys); the solo announcement, bootstrap, and close are always home.

**Divisions (statement escalation)**: `GroundBassConfiguration.Divisions` (default on; requires `VoiceRhythmConfiguration.Enabled`, the role machinery's master switch) gives the ground its textural arc. After the walk wins and the announcement is stripped, `GroundBassComposer` records division roles by reference into the shared `VoiceRhythmLedger`: every ground-line note held (the role-aware sustain gate then ties the bass's sustain-eligible repeated pairs deterministically — the announcement fully, statements wherever the suspension pass leaves a pair whole), every accompanied upper note carrying its statement's intensity from `GroundBassDivisionScheduler` (a draw-free, END-anchored ramp: Calm 30 → Peak 140, the final statement always peaking), and one rotating upper voice per statement florid. Only the SUBDIVIDING-tier gates scale by intensity (decoration coverage is near-saturated, so scaling every figure just reshuffles processor competition — measured); the sustain gate never scales (a calm statement's reappearing ties are its calm). After dynamics, a draw-free terrace pass offsets each statement's velocities downward toward zero at the close (−5 → 0, exactly once per note — an in-engine offset would compound through the velocity walk). The announcement and close never escalate; a bass-only ground keeps only the tread; divisions-off (or voice-rhythm-off) renders byte-identical to the pre-divisions form (voice-rhythm-off is decided in-process; divisions-off was decided by a cross-branch hash harness at ship time and is held since by the scheduler's decline pins), and the fugue never records division roles.

**Textures (fugal accompaniment)**: `CompositionConfiguration.TextureConfiguration` (param #21, default None; the Texture select shown under the Fugue form; requires `VoiceRhythmConfiguration.Enabled` and ≥2 voices) trades the fugal body's imitative fabric for melody-over-accompaniment. The `VoiceRhythmScheduler`'s texture mode supersedes its per-block rotation (both rotating answers decline) with one static register-derived assignment — highest voice = MELODY (florid store), lowest = FIGURATION (its own 4th ledger store), middles = PADS (held store; ties are opportunistic, no walk pins) — ordered by MinNote desc, MaxNote desc, then Instrument. `VoiceRhythmPolicyTransformer` derives per-processor texture weights at engine build (in-family 95 / out-of-family 0 / None = stock; families are onset-spacing-uniform: Walking = {RepeatedNote, PassingTone} quarter tread, BrokenChord = {six octave-pedal figures + Pedal} eighth pattern, Chordal = empty — user-disabling a family's figures degrades that texture toward Chordal), and the gate chain resolves held → texture (short-circuit, NEVER ×intensity) → florid → ×intensity, one draw per item. The body walk records roles per beat, and after phrasing the composer re-records the phraser's deep-copied restatements (idempotent on the reference-keyed stores): an accompaniment copy sheds its copied-in figures first (theme-phrase restatements carry the exposition's stock decoration by value) so the second decoration pass re-clothes it in-family, while the melody keeps its copied figures. Under an active texture the whole-composition ornamentation pass decorates melody-first (register order) so the cleaners' just-decorated-loses rule sacrifices the accompaniment within each pass; the sustain pass keeps raw order (its gate draws per item — reordering would shift the shared stream), and `EndingComposer`'s whole-composition calls re-order harmlessly (no recorded notes). After dynamics, `Composer.ApplyTextureProminence` offsets accompaniment velocities by −8 (clamped to the instrument window, sub-notes mirrored, exactly once, draw-free, gated on the scheduler so rotation-held notes are never offset). The exposition and ending stay stock; a ground render that produces a plan never records texture; the unplannable-ground fallback fugue TAKES the texture (it is a fugue — the pattern popover documents this since the Texture control is hidden under the ground form); texture-off (None / voice-rhythm-off / <2 voices) renders byte-identical to pre-texture (decided by a cross-branch hash harness at ship time, held since by the scheduler's decline pins and the empty-store weight identity). Legacy saves load as None; Randomize preserves the texture, Reset clears it.

### Key Abstractions

- **`CompositionStrategy`** — Uses `IChordChoiceRepository` to enumerate possible next chords, validates them against `ICompositionRule`, and does a look-ahead search to ensure the composition doesn't paint itself into a corner.
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

## Determinism in Seeded Tests

- `ShuffleOrnamentationProcessors` defaults to `true` and is deliberately **not** seed-reproducible; seeded or comparative tests must set it to `false`.
- Seeded walks differ across operating systems: assert seed-sweep existence properties (`Enumerable.Range(1, N).Any(...)`), never per-seed outcome pins; per-seed pins are only safe for properties that hold for every seed.
- A/B comparisons between two seeded runs must be draw-aligned: disabling a feature outright (`Enabled: false`) removes its RNG draws and shifts every later pass's decisions, so compare against a control that consumes identical draws (e.g. the feature enabled at `Probability: 0`) or compare only divergence-robust properties.

---
paths:
  - "tests/BaroquenMelody.Architecture.Tests/**"
---

# Writing and maintaining architecture rules

This loads only when you work inside `tests/BaroquenMelody.Architecture.Tests`. The conventions the rules enforce are summarised in CLAUDE.md ("Architecture Tests"); this file is about the rules themselves.

## Layout

- `BaroquenMelodyArchitecture` holds the single cached `Architecture` — eight assemblies via `Assembly.Load` (the console app's assembly name is literally `baroquen-melody`). `AssemblyCompositionTests` asserts the eight names so a missing or renamed assembly cannot silently shrink every rule's subject set (ArchUnitNET's positive-results check only catches a completely empty subject).
- One fixture per tier: `LayerDependencyTests` (A-1..A-8 assembly boundaries), `LibraryStructureTests` (L-1..L-18), `FrontendStructureTests` (F-1..F-3), `TestSuiteConventionTests` (T-1..T-4 plus the T-4 baseline guard), `PositiveControlTests` (rules that MUST fail — they prove each detection mechanism can fire).
- The C# namespace is `BaroquenMelody.ArchitectureTests`; the folder, csproj and assembly stay `BaroquenMelody.Architecture.Tests` like every other test project. An `Architecture` namespace segment shadows `ArchUnitNET.Domain.Architecture` (CS0118) and a `using` alias cannot fix it. Keep `using Assembly = System.Reflection.Assembly;` where both `Assembly` types are in scope.

## Shape of a rule

- One `[Test]` per rule, named as the sentence it asserts. Always end with `.Because("…")` before `.Check(Architecture)` — it is the first line of the CI failure message, and for frozen rules it is part of the store key.
- Scope the subject with `ResideInAssembly(BaroquenMelodyArchitecture.X)` first, never by namespace prefix alone: the console app declares its own `BaroquenMelody.Infrastructure.FileSystem` namespace.
- Anchor namespace regexes: `ResideInNamespaceMatching(@"^BaroquenMelody\.Library\.Rules(\..+)?$")`.
- Prefer `NotBePublic()` over `BeInternal()` when the intent is "not part of the surface" — private nested helpers are legal.
- Address internal types as domain objects: `Architecture.Interfaces.First(i => i.FullName == "…")`, `Architecture.Types.First(...)`. ArchUnitNET 0.13 removed the string overloads of `ImplementInterface` / `AreDeclaredIn`; a stale name now throws at lookup instead of matching nothing. Call the lookup inside the test (or a helper), not in a static initializer — a throwing type initializer poisons every test in the fixture.
- Query style (`Architecture.Types.Where(...)` + `Assert.That(offenders, Is.Empty, …)`) is fine when the fluent API cannot express the rule; L-15 is the model.
- `dotnet test` builds Debug; keep it that way (Release IL drops some detectable dependencies — local-variable initialisers, casts, `typeof`).

## ArchUnitNET traps — every one of these was hit here

1. **A rule whose SUBJECT matches nothing FAILS** ("The rule requires positive evaluation"). That is a feature. Never add `WithoutRequiringPositiveResults()` to make a rule pass; fix the predicate.
2. **A rule whose TARGET is an object provider naming an external type is silently EMPTY.** `Types()`, `Classes()` and `MethodMembers()` enumerate loaded assemblies only, so `NotDependOnAny(Types().That().HaveFullName("System.Random"))` passes vacuously and the positive-results check does not protect condition position. Use `NotDependOnAnyTypesThat().HaveFullName(...)` / `.HaveFullNameMatching(...)` / `.ResideInNamespaceMatching(...)` for anything outside the eight loaded assemblies. `PositiveControlTests.An_object_provider_naming_an_external_type_is_silently_empty` pins this.
3. **Assignability stops at loaded assemblies.** `AreAssignableTo(typeof(ComponentBase))` matched 16 of 29 components because `FluxorComponent`, `MudComponentBase` and `LayoutComponentBase` live in unloaded packages. Use the interface every Razor component implements (`ImplementInterface(typeof(IComponent))`) or load the package.
4. **`CallAny` cannot see calls to generic methods** — the call target is a generic-instance member, not the open declaration `MethodMembers()` enumerates. Key such rules off the type-level dependency (`DependOnAny(Types().That().HaveFullName("…StateExtensions"))`, as F-3 does) or use `FollowCustomPredicate` over `GetMethodCallDependencies()`.
5. **Member names include the parameter list**: `DisposeAsync()`, `.ctor(BaroquenMelody.Library.Configurations.CompositionConfiguration)`, ``ObserveChanges(Fluxor.IState`1<TState>)``.
6. **`with { }` on a sealed record compiles to a copy-constructor call.** Constructor-ban rules must exclude `.ctor(<SelfType>)` (T-4 does).
7. **Slice patterns `(*)` and `(**)` behave identically here** (one slice per full sub-namespace; 116 once the test assemblies are loaded). Cycle rules are deliberately not used: the Library namespace graph is cyclic by design (the composition root sits in `Extensions`; `Rules ↔ Rules.Harmonic`, `Ornamentation ↔ Ornamentation.Engine.*`).
8. **StyleCop in this project**: usings strictly alphabetical with `System.*` AFTER package namespaces (`SA1210`); private helpers after the tests (`SA1202`); `using static` last. The project inherits bUnit's AngleSharp advisory through `App.Components.Tests` — keep the `NuGetAuditSuppress` in the csproj.
9. **Attribute dependencies count as dependencies** (`[FeatureState]`, `[TestFixture]`, `[Explicit]`), which is what T-3 and the `HaveAnyAttributes` predicates rely on; `const` coupling is invisible (inlined).

## Frozen rules

- `FreezingArchRule.Freeze(rule, JsonViolationStore)` baselines the CURRENT violations by type full name; only new types fail. Freeze **re-baselines silently** when the store has no entry for the rule's exact description (the `Because()` text is part of that key; the stored copy is truncated with `…`) and **rewrites the store on every evaluation**.
- Every frozen rule therefore ships with the two guards T-4 has — copy them: `Assert.That(store.RuleAlreadyFrozen(rule), Is.True, …)` before `Freeze`, and a type-initialisation snapshot of the committed baseline compared against `rule.Evaluate(Architecture)` so an un-committed shrink fails CI.
- Fixing, renaming or deleting a baselined type: run the tests, commit the shrunk JSON. Changing a frozen rule's `Because()`: remove its entry, run, commit — deliberately, in the same PR. Never hand-edit the JSON.
- Freezing is per TYPE: a baselined fixture may add call sites and stay green. If per-site protection matters, prefer the raw rule plus an explicit `DoNotHaveFullName(...)` allowlist that shrinks by PR.
- The baseline file is byte-stable across runs on Windows and Linux, so local runs do not dirty the tree; if `git status` ever shows it modified after a run, something in the baseline changed — read the diff, do not discard it.

## Extending coverage

- A new detection mechanism (attribute dependency, external type, cross-assembly reference, generic call, …) ⇒ add a `PositiveControlTests` case that must fail, so a vacuous pass is caught next time.
- A new assembly ⇒ `Assembly.Load` it in `BaroquenMelodyArchitecture`, add it to the expected-names list, add the `ProjectReference`. The MAUI host cannot be loaded (platform TFMs); gating it needs a Windows lane with the MAUI workloads.
- Do NOT write these (the codebase contradicts them by design): "Infrastructure types are internal" (all 19 are public ports used by the Library and both hosts); "the UI must not use Infrastructure" (`ObserveChanges()` lives there); "DryWetMidi stays behind the MIDI boundary" (it is the Library's core vocabulary); namespace acyclicity.
- Known follow-up: L-14 cannot ban `System.IO.FileInfo` until `SavedCompositionConfiguration` and `CompositionConfigurationPersistenceService` switch to `IFileInfo`.

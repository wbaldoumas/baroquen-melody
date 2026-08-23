using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation;

/// <summary>
///     End-to-end coverage for prepared suspensions. The applicator draws randomness only after the walk is
///     fully composed, so enabling suspensions must never change which chords are composed - only how the
///     barline is articulated - and the rendered figure must be audible in the MIDI: a tied preparation
///     spanning a beat and a half whose end abuts its own half-beat resolution a step below. Trill-style
///     seed sweeps are used instead of per-seed pins because seeded walks differ across operating systems.
/// </summary>
[TestFixture]
[Category("Composition")]
internal sealed class SuspensionCompositionTests
{
    private const int SeedCount = 5;

    // MIDI geometry under the generator's 96-ticks-per-quarter time division with half-note beats: a tied
    // preparation spans a beat and a half (288 ticks) and its delayed resolution fills the remaining half
    // beat (96 ticks).
    private const long PreparationTickLength = 288;

    private const long ResolutionTickLength = 96;

    [Test]
    public void Compose_WithSuspensionsEnabledVersusDisabled_ChangesArticulationButNotTheWalk()
    {
        // arrange
        var enabled = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };
        var disabled = enabled with { SuspensionConfiguration = new SuspensionConfiguration(Enabled: false, Probability: 0) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var suspended = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var plain = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            // assert - the rendered notes differ, but the composition spans identical time: suspensions only
            // re-articulate the barline, they never change the composed walk
            suspended.Should().NotEqual(plain, "suspensions must change the rendered music for seed {0}", seed);

            var suspendedFinalTick = suspended.Max(static note => note.Time + note.Length);
            var plainFinalTick = plain.Max(static note => note.Time + note.Length);

            suspendedFinalTick.Should().Be(plainFinalTick, "suspensions must not change the composition's duration for seed {0}", seed);
        }
    }

    [Test]
    public void Compose_WithSuspensionsEnabled_ArticulatesTheThemeExpositionsOpeningBarlines()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

        // act & assert - the theme exposition is prepended after the body pipeline runs, so a figure
        // resolving inside the opening two measures can only come from the exposition's own suspension
        // pass: the earliest body figure sits past the exposition, at least 1248 ticks in
        Enumerable.Range(1, 12)
            .Any(seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                return notes
                    .Where(static note => note.Length >= PreparationTickLength)
                    .Any(preparation => notes.Any(resolution =>
                        resolution.Time == preparation.Time + preparation.Length &&
                        resolution.Length == ResolutionTickLength &&
                        preparation.NoteNumber - resolution.NoteNumber is 1 or 2 &&
                        resolution.Time <= 960));
            })
            .Should().BeTrue("some seeded composition must render a suspension inside the theme exposition");
    }

    [Test]
    public void Compose_WithSuspensionsEnabled_RendersTiedPreparationsWithDelayedStepwiseResolutions()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

        // act & assert - some seed must render the full figure; the sweep stops at the first hit
        Enumerable.Range(1, 12)
            .Any(seed =>
            {
                var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

                // the preparation may render at 288 ticks, or longer when the sustain pass absorbed it into a
                // same-pitch predecessor's tie - either way it ends exactly where its resolution begins
                return notes
                    .Where(static note => note.Length >= PreparationTickLength)
                    .Any(preparation => notes.Any(resolution =>
                        resolution.Time == preparation.Time + preparation.Length &&
                        resolution.Length == ResolutionTickLength &&
                        preparation.NoteNumber - resolution.NoteNumber is 1 or 2));
            })
            .Should().BeTrue("some seeded composition must render a tied preparation resolving down by step");
    }
}

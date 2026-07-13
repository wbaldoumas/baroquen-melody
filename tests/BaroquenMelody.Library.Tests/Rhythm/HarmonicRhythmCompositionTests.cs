using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rhythm;

/// <summary>
///     End-to-end coverage for the harmonic rhythm: the default hold schedule must actually change the rendered
///     composition and must surface as two-beat notes in the MIDI output. Determinism under a seed and the musical
///     invariants are exercised by the engine-wide suites, since the harmonic rhythm is enabled by default.
/// </summary>
[TestFixture]
internal sealed class HarmonicRhythmCompositionTests
{
    private const int SeedCount = 5;

    /// <summary>
    ///     The rendered length of a chord held across two beats: the default note time span is a half note, which
    ///     the MIDI generator renders as 192 ticks, so a two-beat hold sounds as a single 384-tick note.
    /// </summary>
    private const long TwoBeatTickLength = 384;

    [Test]
    public void Compose_WithHarmonicRhythmEnabledVersusDisabled_ProducesDifferentMusic()
    {
        // arrange: the default-enabled harmonic rhythm holds weak beats in every phrase-interior measure, so unlike
        // probability-gated features, every seed must render differently with it disabled.
        var enabled = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };
        var disabled = enabled with { HarmonicRhythmConfiguration = new HarmonicRhythmConfiguration(Enabled: false) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var held = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var freshlyComposed = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            // assert
            held.Should().NotEqual(freshlyComposed, "the deterministic hold schedule must change the rendered composition for seed {0}", seed);
        }
    }

    [Test]
    public void Compose_WithHarmonicRhythmEnabled_RendersTwoBeatHolds()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

            // assert
            notes.Should().Contain(
                static note => note.Length == TwoBeatTickLength,
                because: "held beats must sound as single notes spanning both beats for seed {0}",
                seed);
        }
    }
}

using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.MusicTheory;

/// <summary>
///     End-to-end coverage for tonicization. The applicator draws randomness only after the walk is fully
///     composed and judges eligibility on diatonic chord numbers, so enabling it can never change which
///     chords are composed or how long the piece is - only which thirds are raised. In Ionian the v1
///     license table is empty and the applicator consumes nothing, so output is byte-identical either
///     way. Trill-style seed sweeps are used instead of per-seed pins because seeded walks differ across
///     operating systems.
/// </summary>
[TestFixture]
internal sealed class TonicizationCompositionTests
{
    private const int SeedCount = 5;

    // In A Aeolian the licensed alterations are the raised thirds G# (from v), C# (from i), and F#
    // (from iv, also the courtesy neighbor below a raised G#).
    private static readonly HashSet<NoteName> LicensedPitchClasses = [NoteName.GSharp, NoteName.CSharp, NoteName.FSharp];

    [Test]
    public void Compose_InIonian_IsByteIdenticalWhetherTonicizationIsEnabledOrDisabled()
    {
        // arrange - the v1 license table is Aeolian-only: in Ionian the pass returns before consuming
        // any randomness, so the output must be identical note for note
        var enabled = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };
        var disabled = enabled with { TonicizationConfiguration = new TonicizationConfiguration(Enabled: false, Probability: 0) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var tonicized = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var plain = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            // assert
            tonicized.Should().Equal(plain, "tonicization must be inert outside Aeolian for seed {0}", seed);
        }
    }

    [Test]
    public void Compose_InAeolian_NeverChangesTheDuration()
    {
        // arrange
        var enabled = TestCompositionConfigurations.Get(3, 10, tonic: NoteName.A, mode: Mode.Aeolian) with { ShuffleOrnamentationProcessors = false };
        var disabled = enabled with { TonicizationConfiguration = new TonicizationConfiguration(Enabled: false, Probability: 0) };

        for (var seed = 1; seed <= SeedCount; seed++)
        {
            // act
            var tonicized = SeededComposition.Notes(SeededComposition.Compose(enabled, seed));
            var plain = SeededComposition.Notes(SeededComposition.Compose(disabled, seed));

            // assert - the pass mutates pitches (the existence sweep below proves that it does) and may
            // reset a clashing ornament, but every beat keeps its total time, so the duration is exact
            var tonicizedFinalTick = tonicized.Max(static note => note.Time + note.Length);
            var plainFinalTick = plain.Max(static note => note.Time + note.Length);

            tonicizedFinalTick.Should().Be(plainFinalTick, "tonicization must not change the composition's duration for seed {0}", seed);
        }
    }

    [Test]
    public void Compose_InAeolian_RendersARaisedThirdForSomeSeed()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10, tonic: NoteName.A, mode: Mode.Aeolian) with { ShuffleOrnamentationProcessors = false };

        // act & assert - some seeded composition must sound a licensed chromatic pitch: the engine's
        // first real dominant in minor
        Enumerable.Range(1, 12)
            .Any(seed => SeededComposition.Notes(SeededComposition.Compose(configuration, seed))
                .Any(static note => LicensedPitchClasses.Contains((NoteName)(note.NoteNumber % 12))))
            .Should().BeTrue("some seeded composition must render a raised third");
    }
}

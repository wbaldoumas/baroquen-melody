using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Builders;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics.Engine.Builders;

[TestFixture]
internal sealed class DynamicsEngineBuilderTests
{
    [Test]
    public void Build_produces_engine_that_accents_strong_beats()
    {
        // arrange: a single instrument sustained across one four-beat measure.
        var configuration = TestCompositionConfigurations.Get(1);
        var maxVelocity = configuration.InstrumentConfigurationsByInstrument[Instrument.One].MaxVelocity;

        var engine = new DynamicsEngineBuilder(configuration, new SeededRandomProvider(1)).Build();
        var applicator = new DynamicsApplicator(configuration, engine);

        Beat Beat() => new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]));

        var composition = new Composition([
            new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour)
        ]);

        // act
        applicator.Apply(composition);

        var velocities = composition.Measures
            .SelectMany(static measure => measure.Beats)
            .Select(static beat => beat[Instrument.One].Velocity)
            .ToList();

        // assert: the strong downbeat is pushed to the instrument's ceiling, while the following weak beat is not.
        velocities[0].Should().Be(maxVelocity, "the downbeat is a strong beat and should be accented to the velocity ceiling");
        velocities[1].Should().BeLessThan(maxVelocity, "the second beat is weak and should not be accented");
    }

    // If the metric accent were added to the stored velocity that the next beat's random walk steps from, the accents
    // would accumulate and the walk would pin at the ceiling: with the default 50..60 window every note after the first
    // downbeat would sit at 59 or 60 for EVERY seed. The sweep asserts the existence of a seed whose velocities escape
    // the top two values of the window (a single pinned seed is not safe here: a walk that happens to hover at 58..60
    // legitimately clamps both accents, so the property is seeded-existence, not per-seed).
    [Test]
    public void Build_produces_engine_whose_velocities_use_the_instrument_range_instead_of_pinning_at_the_ceiling()
    {
        // arrange: a single instrument sustained across two four-beat measures.
        var configuration = TestCompositionConfigurations.Get(1);
        var maxVelocity = configuration.InstrumentConfigurationsByInstrument[Instrument.One].MaxVelocity;

        Beat Beat() => new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]));

        // act & assert: some seed must use the configured range rather than saturating at the ceiling.
        Enumerable.Range(1, 20).Any(
            seed =>
            {
                var engine = new DynamicsEngineBuilder(configuration, new SeededRandomProvider(seed)).Build();
                var applicator = new DynamicsApplicator(configuration, engine);

                var composition = new Composition([
                    new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour),
                    new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour)
                ]);

                applicator.Apply(composition);

                var velocities = composition.Measures
                    .SelectMany(static measure => measure.Beats)
                    .Select(static beat => beat[Instrument.One].Velocity)
                    .ToList();

                var weakBeatVelocities = velocities.Where(static (_, index) => index % 2 == 1).ToList();

                return velocities.Distinct().Count() > 2 && weakBeatVelocities.Exists(velocity => velocity < maxVelocity - 1);
            })
            .Should().BeTrue("some seeded walk should escape the top two values of the configured window and leave a weak beat audibly below the accented downbeats");
    }

    // On a window wide enough that nothing clamps, the accent contract is exact: an accented beat carries the
    // un-accented base plus its accent, and the following beat's walk steps from the base, not from the accented value.
    // These are every-seed properties: the downbeat takes no draw, and the walk moves exactly one unit per beat.
    [Test]
    public void Build_produces_engine_whose_accent_is_local_emphasis_and_whose_walk_continues_from_the_unaccented_base()
    {
        // arrange: a 20..100 velocity window, so the initial velocity sits at 20 + 80 * 0.75 = 80 with headroom above.
        var configuration = TestCompositionConfigurations.Get(1, minVelocity: new SevenBitNumber(20), maxVelocity: new SevenBitNumber(100));

        var engine = new DynamicsEngineBuilder(configuration, new SeededRandomProvider(1)).Build();
        var applicator = new DynamicsApplicator(configuration, engine);

        Beat Beat() => new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]));

        var composition = new Composition([
            new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour),
            new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour)
        ]);

        // act
        applicator.Apply(composition);

        var velocities = composition.Measures
            .SelectMany(static measure => measure.Beats)
            .Select(static beat => (int)beat[Instrument.One].Velocity)
            .ToList();

        // assert: the downbeat is exactly base + strong accent, and the next beat walks one unit off the base itself.
        velocities[0].Should().Be(88, "the strong downbeat should carry the initial velocity of 80 plus the strong accent of 8");
        velocities[1].Should().BeOneOf([79, 81], "the beat after the downbeat should step one unit from the un-accented base of 80, not from the accented value");
        velocities.Should().OnlyContain(velocity => velocity <= 95, $"eight beats of a one-unit walk from 80 plus a +8 accent can never reach the ceiling of 100 (velocities: {string.Join(", ", velocities)})");
    }
}

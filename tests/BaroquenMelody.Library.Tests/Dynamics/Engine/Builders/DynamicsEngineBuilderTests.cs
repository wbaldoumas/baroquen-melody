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

    // Audit: dynamics-midi-instruments-1
    // The metric accent is added to the stored velocity that the next beat's random walk steps from, so the accents
    // accumulate and the walk pins at the ceiling: with the default 50..60 window every note after the first downbeat
    // sits at 59 or 60. The configured range should actually be used and weak beats should fall below the accented pair.
    [Test]
    public void Build_produces_engine_whose_velocities_use_the_instrument_range_instead_of_pinning_at_the_ceiling()
    {
        // arrange: a single instrument sustained across two four-beat measures.
        var configuration = TestCompositionConfigurations.Get(1);
        var maxVelocity = configuration.InstrumentConfigurationsByInstrument[Instrument.One].MaxVelocity;

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
            .Select(static beat => beat[Instrument.One].Velocity)
            .ToList();

        var weakBeatVelocities = velocities.Where(static (_, index) => index % 2 == 1).ToList();

        // assert: the velocities must not collapse onto the top two values of the configured window.
        velocities.Distinct().Should().HaveCountGreaterThan(2, $"the dynamics should use the configured range rather than saturating at the ceiling (velocities: {string.Join(", ", velocities)})");
        weakBeatVelocities.Should().Contain(velocity => velocity < maxVelocity - 1, $"some weak beat should sit audibly below the accented downbeats (velocities: {string.Join(", ", velocities)})");
    }
}

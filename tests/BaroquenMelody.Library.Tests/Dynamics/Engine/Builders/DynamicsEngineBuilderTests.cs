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
}

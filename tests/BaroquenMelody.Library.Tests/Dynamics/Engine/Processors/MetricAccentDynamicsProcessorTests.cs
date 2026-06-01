using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Processors;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics.Engine.Processors;

[TestFixture]
internal sealed class MetricAccentDynamicsProcessorTests
{
    private const int StrongAccent = 8;

    private const int MediumAccent = 4;

    // TestCompositionConfigurations.Get(1) yields Instrument.One with the default 50/60 velocity bounds.
    private static readonly MetricAccentDynamicsProcessor Processor = new(TestCompositionConfigurations.Get(1), StrongAccent, MediumAccent);

    [Test]
    [TestCase(MetricStrength.Strong, 50, 58)]
    [TestCase(MetricStrength.Strong, 55, 60)]
    [TestCase(MetricStrength.Strong, 60, 60)]
    [TestCase(MetricStrength.Medium, 50, 54)]
    [TestCase(MetricStrength.Medium, 58, 60)]
    [TestCase(MetricStrength.Weak, 50, 50)]
    [TestCase(MetricStrength.Weak, 60, 60)]
    public void Process_accents_velocity_by_metric_strength_and_clamps(MetricStrength strength, byte baseVelocity, byte expectedVelocity)
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(baseVelocity) };

        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([note])),
            ProcessedInstruments = [],
            CurrentBeatStrength = strength
        };

        // act
        Processor.Process(item);

        // assert
        item.CurrentBeat[Instrument.One].Velocity.Should().Be(new SevenBitNumber(expectedVelocity));
    }

    [Test]
    public void Process_accents_ornamentation_velocities_to_match()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(50) };
        var ornamentation = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth) { Velocity = new SevenBitNumber(50) };

        note.Ornamentations.Add(ornamentation);

        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([note])),
            ProcessedInstruments = [],
            CurrentBeatStrength = MetricStrength.Strong
        };

        // act
        Processor.Process(item);

        // assert
        note.Velocity.Should().Be(new SevenBitNumber(58));
        ornamentation.Velocity.Should().Be(new SevenBitNumber(58));
    }
}

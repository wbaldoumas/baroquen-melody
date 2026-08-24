using Atrea.PolicyEngine;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics;

[TestFixture]
internal sealed class DynamicsApplicatorTests
{
    private IPolicyEngine<DynamicsApplicationItem> _mockPolicyEngine = null!;
    private DynamicsApplicator _dynamicsApplicator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockPolicyEngine = Substitute.For<IPolicyEngine<DynamicsApplicationItem>>();

        _dynamicsApplicator = new DynamicsApplicator(
            TestCompositionConfigurations.Get(2),
            _mockPolicyEngine
        );
    }

    [Test]
    public void Apply_adds_dynamics_to_composition()
    {
        // arrange
        var composition = new Composition(
            [
                new Measure(
                    [
                        new Beat(
                            new BaroquenChord(
                                [
                                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                                    new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)
                                ]
                            )
                        ),
                        new Beat(
                            new BaroquenChord(
                                [
                                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                                    new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)
                                ]
                            )
                        ),
                        new Beat(
                            new BaroquenChord(
                                [
                                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                                    new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)
                                ]
                            )
                        ),
                        new Beat(
                            new BaroquenChord(
                                [
                                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                                    new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)
                                ]
                            )
                        )
                    ],
                    Meter.FourFour
                )
            ]
        );
        byte velocity = 1;

        // every (instrument, beat) pair reaches the engine as its own item, instrument by instrument, beat by beat
        _mockPolicyEngine.When(policyEngine => policyEngine.Process(Arg.Any<DynamicsApplicationItem>()))
            .Do(callInfo =>
                {
                    var dynamicsApplicationItem = callInfo.Arg<DynamicsApplicationItem>();

                    dynamicsApplicationItem.CurrentBeat[dynamicsApplicationItem.Instrument].Velocity = new SevenBitNumber(velocity++);
                }
            );

        var expectedInstrumentOneVelocities = new[]
        {
            new SevenBitNumber(1),
            new SevenBitNumber(2),
            new SevenBitNumber(3),
            new SevenBitNumber(4)
        };

        var expectedInstrumentTwoVelocities = new[]
        {
            new SevenBitNumber(5),
            new SevenBitNumber(6),
            new SevenBitNumber(7),
            new SevenBitNumber(8)
        };

        // act
        _dynamicsApplicator.Apply(composition);

        // assert
        _mockPolicyEngine.Received(8).Process(Arg.Any<DynamicsApplicationItem>());

        var actualInstrumentOneVelocities = composition.Measures
            .SelectMany(measure => measure.Beats)
            .Select(beat => beat[Instrument.One].Velocity)
            .ToArray();

        var actualInstrumentTwoVelocities = composition.Measures
            .SelectMany(measure => measure.Beats)
            .Select(beat => beat[Instrument.Two].Velocity)
            .ToArray();

        actualInstrumentOneVelocities.Should().Equal(expectedInstrumentOneVelocities);
        actualInstrumentTwoVelocities.Should().Equal(expectedInstrumentTwoVelocities);
    }

    [Test]
    public void Apply_threads_metric_strength_for_each_beat_within_its_measure()
    {
        // arrange
        var capturedStrengths = new List<MetricStrength>();

        _mockPolicyEngine.When(policyEngine => policyEngine.Process(Arg.Any<DynamicsApplicationItem>()))
            .Do(callInfo => capturedStrengths.Add(callInfo.Arg<DynamicsApplicationItem>().CurrentBeatStrength));

        BaroquenChord Chord() => new([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half)
        ]);

        var composition = new Composition([
            new Measure([new Beat(Chord()), new Beat(Chord()), new Beat(Chord()), new Beat(Chord())], Meter.FourFour)
        ]);

        // act
        _dynamicsApplicator.Apply(composition);

        // assert: each instrument is processed across the four-beat measure as strong - weak - medium - weak.
        var expectedStrengthsPerInstrument = new[] { MetricStrength.Strong, MetricStrength.Weak, MetricStrength.Medium, MetricStrength.Weak };

        capturedStrengths.Should().Equal(
        [
            .. expectedStrengthsPerInstrument,
            .. expectedStrengthsPerInstrument
        ]);
    }
}

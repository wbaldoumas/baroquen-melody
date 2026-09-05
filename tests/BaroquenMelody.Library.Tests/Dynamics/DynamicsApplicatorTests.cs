using Atrea.PolicyEngine;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
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

        // A wide velocity window so the metric accent pass never clamps the tiny mock velocities below.
        _dynamicsApplicator = new DynamicsApplicator(
            TestCompositionConfigurations.Get(2, minVelocity: new SevenBitNumber(0), maxVelocity: new SevenBitNumber(127)),
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

        // The walk assigns 1..4 and 5..8; the post-walk accent pass then adds +8 to the strong first beat and
        // +4 to the medium third beat of the four-beat measure, leaving the weak beats at their walked values.
        var expectedInstrumentOneVelocities = new[]
        {
            new SevenBitNumber(9),
            new SevenBitNumber(2),
            new SevenBitNumber(7),
            new SevenBitNumber(4)
        };

        var expectedInstrumentTwoVelocities = new[]
        {
            new SevenBitNumber(13),
            new SevenBitNumber(6),
            new SevenBitNumber(11),
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

    // The metric accent is a post-walk pass: the walked base velocity is boosted by +8 on strong beats and +4 on
    // medium beats, clamped to the instrument's velocity window, while weak beats keep their walked values.
    // The strong-weak-medium-weak expectations also pin the four-beat metric pattern the accent pass reads.
    [Test]
    [TestCase((byte)50, new[] { 58, 50, 54, 50 })]
    [TestCase((byte)55, new[] { 60, 55, 59, 55 })]
    [TestCase((byte)58, new[] { 60, 58, 60, 58 })]
    [TestCase((byte)60, new[] { 60, 60, 60, 60 })]
    public void Apply_accents_strong_and_medium_beats_after_the_walk_and_clamps_to_the_instrument_window(byte baseVelocity, int[] expectedVelocities)
    {
        // arrange: the default 50..60 velocity window, and a walk that leaves every beat at the same base velocity.
        var dynamicsApplicator = new DynamicsApplicator(TestCompositionConfigurations.Get(1), _mockPolicyEngine);

        _mockPolicyEngine.When(policyEngine => policyEngine.Process(Arg.Any<DynamicsApplicationItem>()))
            .Do(callInfo =>
            {
                var dynamicsApplicationItem = callInfo.Arg<DynamicsApplicationItem>();

                dynamicsApplicationItem.CurrentBeat[dynamicsApplicationItem.Instrument].Velocity = new SevenBitNumber(baseVelocity);
            });

        Beat Beat() => new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]));

        var composition = new Composition([
            new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour)
        ]);

        // act
        dynamicsApplicator.Apply(composition);

        // assert
        var actualVelocities = composition.Measures
            .SelectMany(static measure => measure.Beats)
            .Select(static beat => (int)beat[Instrument.One].Velocity)
            .ToList();

        actualVelocities.Should().Equal(expectedVelocities);
    }

    [Test]
    public void Apply_mirrors_the_accented_velocity_onto_ornamentation_sub_notes()
    {
        // arrange: the default 50..60 velocity window, a walk that leaves every note and sub-note at 50.
        var dynamicsApplicator = new DynamicsApplicator(TestCompositionConfigurations.Get(1), _mockPolicyEngine);

        _mockPolicyEngine.When(policyEngine => policyEngine.Process(Arg.Any<DynamicsApplicationItem>()))
            .Do(callInfo =>
            {
                var dynamicsApplicationItem = callInfo.Arg<DynamicsApplicationItem>();
                var note = dynamicsApplicationItem.CurrentBeat[dynamicsApplicationItem.Instrument];

                note.Velocity = new SevenBitNumber(50);

                foreach (var ornamentation in note.Ornamentations)
                {
                    ornamentation.Velocity = new SevenBitNumber(50);
                }

            });

        Beat Beat()
        {
            var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

            note.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth));

            return new Beat(new BaroquenChord([note]));
        }

        var composition = new Composition([
            new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour)
        ]);

        // act
        dynamicsApplicator.Apply(composition);

        var beats = composition.Measures.SelectMany(static measure => measure.Beats).ToList();

        // assert: the strong downbeat's sub-note carries the accented velocity, while the weak beat's stays at base.
        beats[0][Instrument.One].Velocity.Should().Be(new SevenBitNumber(58));
        beats[0][Instrument.One].Ornamentations[0].Velocity.Should().Be(new SevenBitNumber(58));
        beats[1][Instrument.One].Velocity.Should().Be(new SevenBitNumber(50));
        beats[1][Instrument.One].Ornamentations[0].Velocity.Should().Be(new SevenBitNumber(50));
    }

    [Test]
    public void Apply_skips_instruments_absent_from_an_accented_beat()
    {
        // arrange: a two-instrument configuration, but a composition where only the first instrument ever sounds,
        // as in an exposition before a voice's fugal entry or the ground form's solo announcement.
        var dynamicsApplicator = new DynamicsApplicator(TestCompositionConfigurations.Get(2), _mockPolicyEngine);

        _mockPolicyEngine.When(policyEngine => policyEngine.Process(Arg.Any<DynamicsApplicationItem>()))
            .Do(callInfo =>
            {
                var dynamicsApplicationItem = callInfo.Arg<DynamicsApplicationItem>();

                if (!dynamicsApplicationItem.CurrentBeat.ContainsInstrument(dynamicsApplicationItem.Instrument))
                {
                    return;
                }

                dynamicsApplicationItem.CurrentBeat[dynamicsApplicationItem.Instrument].Velocity = new SevenBitNumber(50);
            });

        Beat Beat() => new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]));

        var composition = new Composition([
            new Measure([Beat(), Beat(), Beat(), Beat()], Meter.FourFour)
        ]);

        // act
        var act = () => dynamicsApplicator.Apply(composition);

        // assert: the absent second instrument is skipped rather than looked up, and the present one is accented.
        act.Should().NotThrow();
        composition.Measures[0].Beats[0][Instrument.One].Velocity.Should().Be(new SevenBitNumber(58));
    }
}

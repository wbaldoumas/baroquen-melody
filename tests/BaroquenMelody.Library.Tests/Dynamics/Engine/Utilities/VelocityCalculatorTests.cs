using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics.Engine.Utilities;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics.Engine.Utilities;

[TestFixture]
internal sealed class VelocityCalculatorTests
{
    private VelocityCalculator _velocityCalculator = null!;

    [SetUp]
    public void SetUp() => _velocityCalculator = new VelocityCalculator();

    [Test]
    public void GetPrecedingVelocity_retrieves_preceding_velocity_when_preceding_beats_is_populated()
    {
        // arrange
        var precedingBeats = new FixedSizeList<Beat>(2)
        {
            new(
                new BaroquenChord(
                    [
                        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) },
                        new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) }
                    ]
                )
            )
        };

        var expectedVelocity = new SevenBitNumber(99);

        // act
        var velocity = _velocityCalculator.GetPrecedingVelocity(precedingBeats, Instrument.One);

        // assert
        velocity.Should().Be(expectedVelocity);
    }

    [Test]
    public void GetPrecedingVelocity_returns_last_ornamentation_velocity_when_ornamentations_are_present()
    {
        // arrange
        var noteWithOrnamentation = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth)
        {
            Velocity = new SevenBitNumber(99),
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth)
                {
                    Velocity = new SevenBitNumber(98)
                },
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth)
                {
                    Velocity = new SevenBitNumber(97)
                },
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Sixteenth)
                {
                    Velocity = new SevenBitNumber(96)
                }
            }
        };

        var precedingBeats = new FixedSizeList<Beat>(2)
        {
            new(
                new BaroquenChord(
                    [
                        noteWithOrnamentation,
                        new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) }
                    ]
                )
            )
        };

        var expectedVelocity = new SevenBitNumber(96);

        // act
        var velocity = _velocityCalculator.GetPrecedingVelocity(precedingBeats, Instrument.One);

        // assert
        velocity.Should().Be(expectedVelocity);
    }

    [Test]
    public void GetPrecedingVelocities_returns_all_velocities_including_ornamentations()
    {
        // arrange
        var noteWithOrnamentation = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth)
        {
            Velocity = new SevenBitNumber(99),
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth)
                {
                    Velocity = new SevenBitNumber(98)
                },
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth)
                {
                    Velocity = new SevenBitNumber(97)
                },
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Sixteenth)
                {
                    Velocity = new SevenBitNumber(96)
                }
            }
        };

        var precedingBeats = new FixedSizeList<Beat>(8)
        {
            new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) }])),
            new(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(9) }])),
            new(new BaroquenChord([noteWithOrnamentation])),
        };

        var expectedVelocities = new List<SevenBitNumber>
        {
            new (96),
            new (97),
            new (98),
            new (99),
            new (9),
            new (99)
        };

        // act
        var velocities = _velocityCalculator.GetPrecedingVelocities(precedingBeats, Instrument.One);

        // assert
        velocities.Should().BeEquivalentTo(expectedVelocities);
    }

    [Test]
    public void GetPrecedingVelocity_throws_exception_when_preceding_beats_is_empty()
    {
        // arrange
        var precedingBeats = new FixedSizeList<Beat>(2);

        // act
        var action = () => _velocityCalculator.GetPrecedingVelocity(precedingBeats, Instrument.One);

        // assert
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetPrecedingVelocity_throws_exception_when_preceding_beats_does_not_contain_instrument()
    {
        // arrange
        var precedingBeats = new FixedSizeList<Beat>(2)
        {
            new(
                new BaroquenChord(
                    [
                        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) },
                        new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) }
                    ]
                )
            )
        };

        // act
        var action = () => _velocityCalculator.GetPrecedingVelocity(precedingBeats, Instrument.Three);

        // assert
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetPrecedingVelocities_throws_exception_when_preceding_beats_is_empty()
    {
        // arrange
        var precedingBeats = new FixedSizeList<Beat>(2);

        // act
        var action = () => _velocityCalculator.GetPrecedingVelocities(precedingBeats, Instrument.One);

        // assert
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void GetPrecedingVelocities_throws_exception_when_preceding_beats_does_not_contain_instrument()
    {
        // arrange
        var precedingBeats = new FixedSizeList<Beat>(2)
        {
            new(
                new BaroquenChord(
                    [
                        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) },
                        new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half) { Velocity = new SevenBitNumber(99) }
                    ]
                )
            )
        };

        // act
        var action = () => _velocityCalculator.GetPrecedingVelocities(precedingBeats, Instrument.Three);

        // assert
        action.Should().Throw<ArgumentException>();
    }
}

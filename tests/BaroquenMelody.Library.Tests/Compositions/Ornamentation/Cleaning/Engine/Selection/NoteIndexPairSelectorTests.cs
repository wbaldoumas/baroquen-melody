#pragma warning disable SA1118

using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Selection;

[TestFixture]
internal sealed class NoteIndexPairSelectorTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Select_selects_expected_indices_based_on_ornamentation_types(
        OrnamentationType primaryOrnamentationType,
        OrnamentationType secondaryOrnamentationType,
        Meter meter,
        IList<NoteIndexPair> expectedIndices)
    {
        // arrange
        var compositionConfiguration = Configurations.GetCompositionConfiguration() with { Meter = meter };
        var musicalTimespanCalculator = new MusicalTimeSpanCalculator();
        var noteOnsetCalculator = new NoteOnsetCalculator(musicalTimespanCalculator, compositionConfiguration);

        var selector = new NoteIndexPairSelector(noteOnsetCalculator);

        // act
        var actualIndices = selector.Select(primaryOrnamentationType, secondaryOrnamentationType).ToList();
        var actualIndicesReversed = selector.Select(secondaryOrnamentationType, primaryOrnamentationType).ToList();

        // assert
        actualIndices.Should().HaveSameCount(expectedIndices);

        for (var i = 0; i < actualIndices.Count; i++)
        {
            actualIndices[i].PrimaryNoteIndex.Should().Be(expectedIndices[i].PrimaryNoteIndex);
            actualIndices[i].SecondaryNoteIndex.Should().Be(expectedIndices[i].SecondaryNoteIndex);
            actualIndices[i].Pulse.Should().Be(expectedIndices[i].Pulse);
            actualIndices[i].OccursOnStrongPulse.Should().Be(expectedIndices[i].OccursOnStrongPulse);
        }

        actualIndicesReversed.Should().HaveSameCount(expectedIndices);

        for (var i = 0; i < actualIndicesReversed.Count; i++)
        {
            actualIndicesReversed[i].PrimaryNoteIndex.Should().Be(expectedIndices[i].SecondaryNoteIndex);
            actualIndicesReversed[i].SecondaryNoteIndex.Should().Be(expectedIndices[i].PrimaryNoteIndex);
            actualIndicesReversed[i].Pulse.Should().Be(expectedIndices[i].Pulse);
            actualIndicesReversed[i].OccursOnStrongPulse.Should().Be(expectedIndices[i].OccursOnStrongPulse);
        }
    }

    [Test]
    public void Select_handles_every_ornamentation_type_in_ever_meter()
    {
        foreach (var ornamentationType in EnumUtils<OrnamentationType>.AsEnumerable())
        {
            foreach (var otherOrnamentationType in EnumUtils<OrnamentationType>.AsEnumerable())
            {
                foreach (var meter in EnumUtils<Meter>.AsEnumerable())
                {
                    var compositionConfiguration = Configurations.GetCompositionConfiguration() with { Meter = meter };
                    var musicalTimespanCalculator = new MusicalTimeSpanCalculator();
                    var noteOnsetCalculator = new NoteOnsetCalculator(musicalTimespanCalculator, compositionConfiguration);
                    var selector = new NoteIndexPairSelector(noteOnsetCalculator);

                    var act = () => selector.Select(ornamentationType, otherOrnamentationType);

                    act.Should().NotThrow();
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        // 4/4 time
        var ornamentationsWithSingleMatchingIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.Pickup, OrnamentationType.Pickup),
            (OrnamentationType.Pickup, OrnamentationType.PassingTone),
            (OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone),
            (OrnamentationType.Pickup, OrnamentationType.DoublePassingTone),
            (OrnamentationType.RepeatedNote, OrnamentationType.PassingTone),
            (OrnamentationType.RepeatedNote, OrnamentationType.Pickup),
            (OrnamentationType.RepeatedNote, OrnamentationType.DoublePassingTone),
            (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone),
            (OrnamentationType.NeighborTone, OrnamentationType.PassingTone),
            (OrnamentationType.NeighborTone, OrnamentationType.Pickup),
            (OrnamentationType.NeighborTone, OrnamentationType.DoublePassingTone),
            (OrnamentationType.NeighborTone, OrnamentationType.RepeatedNote),
            (OrnamentationType.PassingTone, OrnamentationType.PassingTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithSingleMatchingIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.FourFour, new[] { new NoteIndexPair(0, 0, 8) });
        }

        var delayedOrnamentationsWithSingleMatchingIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedPickup),
            (OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedDoublePassingTone),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedPickup),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedDoublePassingTone),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote),
            (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedPickup, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedPickup, OrnamentationType.DelayedPickup),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPickup)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in delayedOrnamentationsWithSingleMatchingIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.FourFour, new[] { new NoteIndexPair(0, 0, 12) });
        }

        var ornamentationsWithTwoMatchingIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithTwoMatchingIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 8),
                    new NoteIndexPair(1, 1, 12)
                });
        }

        var delayedOrnamentationsWithTwoMatchingIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in delayedOrnamentationsWithTwoMatchingIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 12),
                    new NoteIndexPair(1, 1, 14)
                });
        }

        var ornamentationsWithThreeMatchingIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.Run, OrnamentationType.Run),
            (OrnamentationType.Run, OrnamentationType.InvertedTurn),
            (OrnamentationType.Run, OrnamentationType.Turn),
            (OrnamentationType.Run, OrnamentationType.DecorateInterval),
            (OrnamentationType.Run, OrnamentationType.Pedal),
            (OrnamentationType.Turn, OrnamentationType.Turn),
            (OrnamentationType.Turn, OrnamentationType.DecorateInterval),
            (OrnamentationType.Turn, OrnamentationType.Pedal),
            (OrnamentationType.InvertedTurn, OrnamentationType.InvertedTurn),
            (OrnamentationType.InvertedTurn, OrnamentationType.DecorateInterval),
            (OrnamentationType.InvertedTurn, OrnamentationType.Pedal),
            (OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval),
            (OrnamentationType.DecorateInterval, OrnamentationType.Pedal),
            (OrnamentationType.Pedal, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithThreeMatchingIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 4),
                    new NoteIndexPair(1, 1, 8),
                    new NoteIndexPair(2, 2, 12)
                }
            );
        }

        var ornamentationsWithQuarterEighthIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.PassingTone, OrnamentationType.Run),
            (OrnamentationType.PassingTone, OrnamentationType.Turn),
            (OrnamentationType.PassingTone, OrnamentationType.InvertedTurn),
            (OrnamentationType.PassingTone, OrnamentationType.DecorateInterval),
            (OrnamentationType.PassingTone, OrnamentationType.Pedal),
            (OrnamentationType.Pickup, OrnamentationType.Run),
            (OrnamentationType.Pickup, OrnamentationType.Turn),
            (OrnamentationType.Pickup, OrnamentationType.InvertedTurn),
            (OrnamentationType.Pickup, OrnamentationType.DecorateInterval),
            (OrnamentationType.Pickup, OrnamentationType.Pedal),
            (OrnamentationType.RepeatedNote, OrnamentationType.Pedal),
            (OrnamentationType.RepeatedNote, OrnamentationType.DecorateInterval)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithQuarterEighthIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.FourFour, new[] { new NoteIndexPair(0, 1, 8) });
        }

        var ornamentationsWithOffsetIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DecorateInterval, OrnamentationType.DoublePassingTone),
            (OrnamentationType.Pedal, OrnamentationType.DoublePassingTone),
            (OrnamentationType.InvertedTurn, OrnamentationType.DoublePassingTone),
            (OrnamentationType.Run, OrnamentationType.DoublePassingTone),
            (OrnamentationType.Turn, OrnamentationType.DoublePassingTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithOffsetIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[]
                {
                    new NoteIndexPair(1, 0, 8),
                    new NoteIndexPair(2, 1, 12)
                });
        }

        var ornamentationsWithRepeatedNoteIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.Turn, OrnamentationType.RepeatedNote),
            (OrnamentationType.InvertedTurn, OrnamentationType.RepeatedNote),
            (OrnamentationType.Run, OrnamentationType.RepeatedNote),
            (OrnamentationType.DecorateInterval, OrnamentationType.RepeatedNote),
            (OrnamentationType.Pedal, OrnamentationType.RepeatedNote)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithRepeatedNoteIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.FourFour, new[] { new NoteIndexPair(1, 0, 8) });
        }

        var ornamentationsWithSixteenthNoteIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoubleRun, OrnamentationType.DoubleRun),
            (OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn),
            (OrnamentationType.DoubleRun, OrnamentationType.DoubleTurn)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithSixteenthNoteIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 2),
                    new NoteIndexPair(1, 1, 4),
                    new NoteIndexPair(2, 2, 6),
                    new NoteIndexPair(3, 3, 8),
                    new NoteIndexPair(4, 4, 10),
                    new NoteIndexPair(5, 5, 12),
                    new NoteIndexPair(6, 6, 14)
                }
            );
        }

        var ornamentationsWithSixteenthEighthIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoubleRun, OrnamentationType.Run),
            (OrnamentationType.DoubleRun, OrnamentationType.Turn),
            (OrnamentationType.DoubleRun, OrnamentationType.InvertedTurn),
            (OrnamentationType.DoubleRun, OrnamentationType.DecorateInterval),
            (OrnamentationType.DoubleRun, OrnamentationType.Pedal),
            (OrnamentationType.DoubleTurn, OrnamentationType.Run),
            (OrnamentationType.DoubleTurn, OrnamentationType.Turn),
            (OrnamentationType.DoubleTurn, OrnamentationType.InvertedTurn),
            (OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval),
            (OrnamentationType.DoubleTurn, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithSixteenthEighthIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[]
                {
                    new NoteIndexPair(1, 0, 4),
                    new NoteIndexPair(3, 1, 8),
                    new NoteIndexPair(5, 2, 12)
                }
            );
        }

        var mordentOrnamentationIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.Mordent, OrnamentationType.Run),
            (OrnamentationType.Mordent, OrnamentationType.Turn),
            (OrnamentationType.Mordent, OrnamentationType.InvertedTurn),
            (OrnamentationType.Mordent, OrnamentationType.DecorateInterval),
            (OrnamentationType.Mordent, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in mordentOrnamentationIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.FourFour,
                new[] { new NoteIndexPair(1, 0, 4) }
            );
        }

        // 3/4 time
        var ornamentationsWithSingleMatchingIndicesInThreeFour = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.PassingTone, OrnamentationType.PassingTone),
            (OrnamentationType.PassingTone, OrnamentationType.DelayedDoublePassingTone),
            (OrnamentationType.PassingTone, OrnamentationType.NeighborTone),
            (OrnamentationType.PassingTone, OrnamentationType.RepeatedNote),
            (OrnamentationType.Pickup, OrnamentationType.PassingTone),
            (OrnamentationType.Pickup, OrnamentationType.Pickup),
            (OrnamentationType.Pickup, OrnamentationType.DelayedDoublePassingTone),
            (OrnamentationType.Pickup, OrnamentationType.NeighborTone),
            (OrnamentationType.Pickup, OrnamentationType.RepeatedNote),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.NeighborTone),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedNote),
            (OrnamentationType.NeighborTone, OrnamentationType.NeighborTone),
            (OrnamentationType.NeighborTone, OrnamentationType.RepeatedNote),
            (OrnamentationType.RepeatedNote, OrnamentationType.RepeatedNote)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithSingleMatchingIndicesInThreeFour)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.ThreeFour, new[] { new NoteIndexPair(0, 0, 16) });
        }

        var delayedOrnamentationsWithSingleMatchingIndicesInThreeFour = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone),
            (OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedRepeatedNote),
            (OrnamentationType.DelayedPickup, OrnamentationType.DelayedPickup),
            (OrnamentationType.DelayedPickup, OrnamentationType.DelayedPassingTone),
            (OrnamentationType.DelayedPickup, OrnamentationType.DelayedNeighborTone),
            (OrnamentationType.DelayedPickup, OrnamentationType.DelayedRepeatedNote),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone),
            (OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote),
            (OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedRepeatedNote)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in delayedOrnamentationsWithSingleMatchingIndicesInThreeFour)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.ThreeFour, new[] { new NoteIndexPair(0, 0, 20) });
        }

        var ornamentationsWithThreeMatchingIndicesInThreeFour = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.Run, OrnamentationType.Run),
            (OrnamentationType.Run, OrnamentationType.Turn),
            (OrnamentationType.Run, OrnamentationType.InvertedTurn),
            (OrnamentationType.Run, OrnamentationType.DecorateInterval),
            (OrnamentationType.Run, OrnamentationType.Pedal),
            (OrnamentationType.Turn, OrnamentationType.Turn),
            (OrnamentationType.Turn, OrnamentationType.InvertedTurn),
            (OrnamentationType.Turn, OrnamentationType.DecorateInterval),
            (OrnamentationType.Turn, OrnamentationType.Pedal),
            (OrnamentationType.InvertedTurn, OrnamentationType.InvertedTurn),
            (OrnamentationType.InvertedTurn, OrnamentationType.DecorateInterval),
            (OrnamentationType.InvertedTurn, OrnamentationType.Pedal),
            (OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval),
            (OrnamentationType.DecorateInterval, OrnamentationType.Pedal),
            (OrnamentationType.Pedal, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithThreeMatchingIndicesInThreeFour)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.ThreeFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 12),
                    new NoteIndexPair(1, 1, 16),
                    new NoteIndexPair(2, 2, 20)
                }
            );
        }

        var ornamentationsWithSixteenthNoteIndicesInThreeFour = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoubleRun, OrnamentationType.DoubleRun),
            (OrnamentationType.DoubleRun, OrnamentationType.DoubleTurn),
            (OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithSixteenthNoteIndicesInThreeFour)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.ThreeFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 10),
                    new NoteIndexPair(1, 1, 12),
                    new NoteIndexPair(2, 2, 14),
                    new NoteIndexPair(3, 3, 16),
                    new NoteIndexPair(4, 4, 18),
                    new NoteIndexPair(5, 5, 20),
                    new NoteIndexPair(6, 6, 22)
                }
            );
        }

        var delayedRunOrnamentationIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DelayedRun, OrnamentationType.Run),
            (OrnamentationType.DelayedRun, OrnamentationType.Turn),
            (OrnamentationType.DelayedRun, OrnamentationType.InvertedTurn),
            (OrnamentationType.DelayedRun, OrnamentationType.DecorateInterval),
            (OrnamentationType.DelayedRun, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in delayedRunOrnamentationIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.ThreeFour,
                new[]
                {
                    new NoteIndexPair(1, 0, 12),
                    new NoteIndexPair(2, 1, 16),
                    new NoteIndexPair(3, 2, 20)
                }
            );
        }

        var doublePassingToneOrnamentationIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone),
            (OrnamentationType.DoublePassingTone, OrnamentationType.Pickup),
            (OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedNote),
            (OrnamentationType.DoublePassingTone, OrnamentationType.NeighborTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in doublePassingToneOrnamentationIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.ThreeFour, new[] { new NoteIndexPair(1, 0, 16) });
        }

        var doublePassingToneOrnamentationIndicesInThreeFour = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in doublePassingToneOrnamentationIndicesInThreeFour)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.ThreeFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 8),
                    new NoteIndexPair(1, 1, 16)
                }
            );
        }

        var delayedDoublePassingToneOrnamentationIndicesInThreeFour = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in delayedDoublePassingToneOrnamentationIndicesInThreeFour)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.ThreeFour,
                new[]
                {
                    new NoteIndexPair(0, 0, 16),
                    new NoteIndexPair(1, 1, 20)
                }
            );
        }

        var ornamentationsWithHalfQuarterEightIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.PassingTone, OrnamentationType.Run),
            (OrnamentationType.PassingTone, OrnamentationType.Turn),
            (OrnamentationType.PassingTone, OrnamentationType.InvertedTurn),
            (OrnamentationType.PassingTone, OrnamentationType.DecorateInterval),
            (OrnamentationType.PassingTone, OrnamentationType.Pedal),
            (OrnamentationType.Pickup, OrnamentationType.Run),
            (OrnamentationType.Pickup, OrnamentationType.Turn),
            (OrnamentationType.Pickup, OrnamentationType.InvertedTurn),
            (OrnamentationType.Pickup, OrnamentationType.DecorateInterval),
            (OrnamentationType.Pickup, OrnamentationType.Pedal),
            (OrnamentationType.RepeatedNote, OrnamentationType.Pedal),
            (OrnamentationType.RepeatedNote, OrnamentationType.DecorateInterval),
            (OrnamentationType.RepeatedNote, OrnamentationType.Run),
            (OrnamentationType.RepeatedNote, OrnamentationType.Turn),
            (OrnamentationType.RepeatedNote, OrnamentationType.InvertedTurn),
            (OrnamentationType.NeighborTone, OrnamentationType.Run),
            (OrnamentationType.NeighborTone, OrnamentationType.Turn),
            (OrnamentationType.NeighborTone, OrnamentationType.InvertedTurn),
            (OrnamentationType.NeighborTone, OrnamentationType.DecorateInterval),
            (OrnamentationType.NeighborTone, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithHalfQuarterEightIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.ThreeFour, new[] { new NoteIndexPair(0, 1, 16) });
        }

        var delayedDoublePassingToneOrnamentationIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Run),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Turn),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.InvertedTurn),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DecorateInterval),
            (OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in delayedDoublePassingToneOrnamentationIndices)
        {
            yield return new TestCaseData(
                primaryOrnamentationType,
                secondaryOrnamentationType,
                Meter.ThreeFour,
                new[]
                {
                    new NoteIndexPair(0, 1, 16),
                    new NoteIndexPair(1, 2, 20)
                }
            );
        }

        var ornamentationsWithQuarterQuarterEighthIndices = new List<(OrnamentationType, OrnamentationType)>
        {
            (OrnamentationType.DoublePassingTone, OrnamentationType.Run),
            (OrnamentationType.DoublePassingTone, OrnamentationType.Turn),
            (OrnamentationType.DoublePassingTone, OrnamentationType.InvertedTurn),
            (OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval),
            (OrnamentationType.DoublePassingTone, OrnamentationType.Pedal)
        };

        foreach (var (primaryOrnamentationType, secondaryOrnamentationType) in ornamentationsWithQuarterQuarterEighthIndices)
        {
            yield return new TestCaseData(primaryOrnamentationType, secondaryOrnamentationType, Meter.ThreeFour, new[] { new NoteIndexPair(1, 1, 16) });
        }

        yield return new TestCaseData(
            OrnamentationType.DoublePassingTone,
            OrnamentationType.DelayedRun,
            Meter.ThreeFour,
            new[]
            {
                new NoteIndexPair(0, 0, 8),
                new NoteIndexPair(1, 2, 16)
            }
        );
    }
}

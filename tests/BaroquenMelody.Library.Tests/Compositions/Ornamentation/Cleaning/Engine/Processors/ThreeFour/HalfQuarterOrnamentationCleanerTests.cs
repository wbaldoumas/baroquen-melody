using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

[TestFixture]
internal sealed class HalfQuarterOrnamentationCleanerTests
{
    private HalfQuarterOrnamentationCleaner _halfQuarterOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _halfQuarterOrnamentationCleaner = new HalfQuarterOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _halfQuarterOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoB3 = new BaroquenNote(Voice.Soprano, Notes.B3, MusicalTimeSpan.Half.Dotted(1));

            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3, MusicalTimeSpan.Half.Dotted(1));
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half.Dotted(1));

            var sopranoC4WithRepeatedNote = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.RepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var sopranoB3WithAscendingPassingTone = new BaroquenNote(sopranoB3)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var sopranoB3WithAscendingDelayedDoublePassingTone = new BaroquenNote(sopranoB3)
            {
                OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var sopranoB3WithUpperNeighborTone = new BaroquenNote(sopranoB3)
            {
                OrnamentationType = OrnamentationType.NeighborTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoD3WithRepeatedNote = new BaroquenNote(altoD3)
            {
                OrnamentationType = OrnamentationType.RepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoE3WithDescendingPassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoE3WithDescendingDelayedDoublePassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoC3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoE3WithLowerNeighborTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.NeighborTone,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoE3WithRepeatedNote = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.RepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoD3WithRepeatedNote),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoD3)
            ).SetName("When repeated notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoD3WithRepeatedNote),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoD3),
                new BaroquenNote(sopranoC4WithRepeatedNote)
            ).SetName("When repeated notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithAscendingPassingTone),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoB3WithAscendingPassingTone),
                new BaroquenNote(altoE3)
            ).SetName("When passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoB3WithAscendingPassingTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoB3WithAscendingPassingTone)
            ).SetName("When passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoE3)
            ).SetName("When delayed double passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoB3WithAscendingDelayedDoublePassingTone)
            ).SetName("When delayed double passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3)
            ).SetName("When neighbor tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoB3WithUpperNeighborTone)
            ).SetName("When neighbor tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithDescendingPassingTone)
            ).SetName("When repeated note conflicts with passing tone, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated note conflicts with passing tone, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone)
            ).SetName("When repeated note conflicts with delayed double passing tone, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated note conflicts with delayed double passing tone, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithLowerNeighborTone)
            ).SetName("When repeated note conflicts with neighbor tone, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated note conflicts with neighbor tone, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoB3),
                new BaroquenNote(altoE3WithDescendingPassingTone)
            ).SetName("When neighbor tone conflicts with passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoB3)
            ).SetName("When neighbor tone conflicts with passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone)
            ).SetName("When neighbor tone conflicts with delayed double passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3WithUpperNeighborTone),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3)
            ).SetName("When neighbor tone conflicts with delayed double passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithAscendingPassingTone),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone)
            ).SetName("When passing tone conflicts with delayed double passing tone, passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3WithAscendingPassingTone),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoB3)
            ).SetName("When passing tone conflicts with delayed double passing tone, passing tone is cleaned.");

            // when notes don't conflict, neither is cleaned
            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithRepeatedNote),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithRepeatedNote)
            ).SetName("When notes don't conflict, neither is cleaned.");
        }
    }
}

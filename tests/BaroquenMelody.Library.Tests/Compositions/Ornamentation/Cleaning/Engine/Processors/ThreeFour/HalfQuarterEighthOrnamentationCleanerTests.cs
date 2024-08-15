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
internal sealed class HalfQuarterEighthOrnamentationCleanerTests
{
    private HalfQuarterEighthOrnamentationCleaner _halfQuarterEighthOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _halfQuarterEighthOrnamentationCleaner = new HalfQuarterEighthOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _halfQuarterEighthOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoD4 = new BaroquenNote(Voice.One, Notes.D4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoE4 = new BaroquenNote(Voice.One, Notes.E4, MusicalTimeSpan.Half.Dotted(1));

            var altoC3 = new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half.Dotted(1));
            var altoD3 = new BaroquenNote(Voice.Two, Notes.D3, MusicalTimeSpan.Half.Dotted(1));
            var altoE3 = new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoF3 = new BaroquenNote(Voice.Two, Notes.F3, MusicalTimeSpan.Half.Dotted(1));
            var altoG3 = new BaroquenNote(Voice.Two, Notes.G3, MusicalTimeSpan.Half.Dotted(1));

            var sopranoC4WithAscendingPassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var sopranoC4WithUpperNeighborTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.NeighborTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var sopranoD4WithRepeatedNote = new BaroquenNote(sopranoD4)
            {
                OrnamentationType = OrnamentationType.RepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var sopranoC4WithAscendingDelayedDoublePassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoE4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoF3WithDescendingRun = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoC3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter.Dotted(1)
            };

            var altoG3WithDescendingRun = new BaroquenNote(altoG3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoF3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter.Dotted(1)
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingRun)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingPassingTone)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingRun)
            ).SetName("When passing tone conflicts with descending run, passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When passing tone conflicts with descending run, passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingRun)
            ).SetName("When neighbor tone conflicts with descending run, neighbor tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When neighbor tone conflicts with descending run, neighbor tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoD4WithRepeatedNote),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoD4),
                new BaroquenNote(altoG3WithDescendingRun)
            ).SetName("When repeated note conflicts with descending run, repeated note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoD4WithRepeatedNote),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoD4)
            ).SetName("When repeated note conflicts with descending run, repeated note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingRun)
            ).SetName("When delayed double passing tone conflicts with descending run, delayed double passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When delayed double passing tone conflicts with descending run, delayed double passing tone is cleaned");
        }
    }
}

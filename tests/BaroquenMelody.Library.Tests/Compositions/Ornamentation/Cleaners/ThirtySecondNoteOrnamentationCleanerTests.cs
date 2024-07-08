using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaners;

[TestFixture]
internal sealed class ThirtySecondNoteOrnamentationCleanerTests
{
    private ThirtySecondNoteOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new ThirtySecondNoteOrnamentationCleaner();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Clean_WhenCalled_CleansOrnamentation(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // act
        _cleaner.Clean(noteA, noteB);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4);
            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4);
            var sopranoE4 = new BaroquenNote(Voice.Soprano, Notes.E4);
            var sopranoF4 = new BaroquenNote(Voice.Soprano, Notes.F4);
            var sopranoG4 = new BaroquenNote(Voice.Soprano, Notes.G4);

            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3);

            var sopranoWithAscendingThirtySecondNotes = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.ThirtySecondNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoE4),
                    new BaroquenNote(sopranoF4),
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoE4),
                    new BaroquenNote(sopranoF4),
                    new BaroquenNote(sopranoG4)
                }
            };

            var altoC3WithoutDissonantNotes = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.ThirtySecondNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoG3)
                }
            };

            var altoC3WithDissonantSecondNote = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.ThirtySecondNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoG3)
                }
            };

            var altoC3WithDissonantFourthNote = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.ThirtySecondNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoC3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoG3)
                }
            };

            var altoC3WithDissonantSixthNote = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.ThirtySecondNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoG3),
                    new BaroquenNote(altoG3)
                }
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3WithoutDissonantNotes),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3WithoutDissonantNotes)
            ).SetName("When thirty-second notes don't conflict, no notes are cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3WithDissonantSecondNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3)
            ).SetName("When thirty-second notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantSecondNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes)
            ).SetName("When thirty-second notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3WithDissonantFourthNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3)
            ).SetName("When thirty-second notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantFourthNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes)
            ).SetName("When thirty-second notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3WithDissonantSixthNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3)
            ).SetName("When thirty-second notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantSixthNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingThirtySecondNotes)
            ).SetName("When thirty-second notes conflict, lower note is cleaned.");
        }
    }
}

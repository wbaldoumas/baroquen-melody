using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors;

[TestFixture]
internal sealed class ThirtySecondSixteenthNoteOrnamentationCleanerTests
{
    private ThirtySecondSixteenthNoteOrnamentationCleaner _cleaner;

    [SetUp]
    public void SetUp() => _cleaner = new ThirtySecondSixteenthNoteOrnamentationCleaner();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Clean_WhenCalled_CleansOrnamentation(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _cleaner.Process(ornamentationCleaningItem);

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

            var sopranoWithAscendingThirtySecondNoteRun = new BaroquenNote(sopranoC4)
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
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoF3)
                }
            };

            var altoC3WithDissonantFirstNote = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoF3)
                }
            };

            var altoC3WithDissonantSecondNote = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3)
                }
            };

            var altoC3WithDissonantThirdNote = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3)
                }
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3WithoutDissonantNotes),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3WithoutDissonantNotes)
            ).SetName("When thirty-second notes don't conflict with sixteenth notes, no notes are cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithoutDissonantNotes),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3WithoutDissonantNotes),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun)
            ).SetName("When thirty-second notes don't conflict with sixteenth notes, no notes are cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3WithDissonantFirstNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3)
            ).SetName("When thirty-second notes conflict with sixteenth notes, then lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantFirstNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun)
            ).SetName("When thirty-second notes conflict with sixteenth notes, then lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3WithDissonantSecondNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3)
            ).SetName("When thirty-second notes conflict with sixteenth notes, then lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantSecondNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun)
            ).SetName("When thirty-second notes conflict with sixteenth notes, then lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3WithDissonantThirdNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3)
            ).SetName("When thirty-second notes conflict with sixteenth notes, then lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantThirdNote),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingThirtySecondNoteRun)
            ).SetName("When thirty-second notes conflict with sixteenth notes, then lower note is cleaned.");
        }
    }
}

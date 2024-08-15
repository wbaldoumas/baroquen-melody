using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.MeterAgnostic;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors.MeterAgnostic;

[TestFixture]
internal sealed class SixteenthNoteOrnamentationCleanerTests
{
    private SixteenthNoteOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new SixteenthNoteOrnamentationCleaner(Configurations.GetCompositionConfiguration());

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
            var sopranoC4 = new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half);
            var sopranoD4 = new BaroquenNote(Voice.One, Notes.D4, MusicalTimeSpan.Half);
            var sopranoE4 = new BaroquenNote(Voice.One, Notes.E4, MusicalTimeSpan.Half);
            var sopranoF4 = new BaroquenNote(Voice.One, Notes.F4, MusicalTimeSpan.Half);
            var sopranoG4 = new BaroquenNote(Voice.One, Notes.G4, MusicalTimeSpan.Half);

            var altoC3 = new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half);
            var altoD3 = new BaroquenNote(Voice.Two, Notes.D3, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Half);
            var altoF3 = new BaroquenNote(Voice.Two, Notes.F3, MusicalTimeSpan.Half);
            var altoG3 = new BaroquenNote(Voice.Two, Notes.G3, MusicalTimeSpan.Half);

            var sopranoWithAscendingDoubleRun = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DoubleRun,
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
                OrnamentationType = OrnamentationType.DoubleRun,
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
                OrnamentationType = OrnamentationType.DoubleRun,
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
                OrnamentationType = OrnamentationType.DoubleRun,
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
                OrnamentationType = OrnamentationType.DoubleRun,
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
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3WithoutDissonantNotes),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3WithoutDissonantNotes)
            ).SetName("When double run notes don't conflict, no notes are cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3WithDissonantSecondNote),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3)
            ).SetName("When double run notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantSecondNote),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingDoubleRun)
            ).SetName("When double run notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3WithDissonantFourthNote),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3)
            ).SetName("When double run notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantFourthNote),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingDoubleRun)
            ).SetName("When double run notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3WithDissonantSixthNote),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3)
            ).SetName("When double run notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantSixthNote),
                new BaroquenNote(sopranoWithAscendingDoubleRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoWithAscendingDoubleRun)
            ).SetName("When double run notes conflict, lower note is cleaned.");
        }
    }
}

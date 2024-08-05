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
internal sealed class SixteenthNoteOrnamentationCleanerTests
{
    private SixteenthNoteOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new SixteenthNoteOrnamentationCleaner();

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

            var altoD4 = new BaroquenNote(Voice.Alto, Notes.D4);
            var altoC4 = new BaroquenNote(Voice.Alto, Notes.C4);
            var altoB3 = new BaroquenNote(Voice.Alto, Notes.B3);
            var altoA3 = new BaroquenNote(Voice.Alto, Notes.A3);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3);
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);

            var sopranoC4WithAscendingSixteenthNotes = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoE4),
                    new BaroquenNote(sopranoF4)
                }
            };

            var altoF3WithDescendingSixteenthNotes = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoC3)
                }
            };

            var altoA3WithAscendingSixteenthNotes = new BaroquenNote(altoA3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoB3),
                    new BaroquenNote(altoC4),
                    new BaroquenNote(altoD4)
                }
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes),
                new BaroquenNote(altoF3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes),
                new BaroquenNote(altoF3)
            ).SetName("When sixteenth notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes)
            ).SetName("When sixteenth notes conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes),
                new BaroquenNote(altoA3WithAscendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes),
                new BaroquenNote(altoA3WithAscendingSixteenthNotes)
            ).SetName("When sixteenth notes don't conflict, no notes are cleaned.");
        }
    }
}

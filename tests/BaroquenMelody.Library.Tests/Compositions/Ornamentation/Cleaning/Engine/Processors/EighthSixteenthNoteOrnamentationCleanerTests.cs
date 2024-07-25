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
internal sealed class EighthSixteenthNoteOrnamentationCleanerTests
{
    private EighthSixteenthNoteOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new EighthSixteenthNoteOrnamentationCleaner();

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

            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3);
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);

            var sopranoC4WithAscendingPassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                }
            };

            var sopranoC4WithAscendingDoublePassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoE4)
                }
            };

            var altoG3WithDescendingSixteenthNotes = new BaroquenNote(altoG3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3)
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

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes)
            ).SetName("When soprano has ascending passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4)
            ).SetName("When soprano has ascending passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes)
            ).SetName("When soprano has ascending double passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4)
            ).SetName("When soprano has ascending double passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingSixteenthNotes)
            ).SetName("When passing tone and sixteenth notes don't conflict, then nothing is cleaned.");
        }
    }
}

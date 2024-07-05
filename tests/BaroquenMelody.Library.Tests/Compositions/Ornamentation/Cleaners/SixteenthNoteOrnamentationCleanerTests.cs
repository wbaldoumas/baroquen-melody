using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaners;

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

            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3);
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);

            var sopranoC4WithAscendingSixteenthNotes = new BaroquenNote(sopranoC4)
            {
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoE4),
                    new BaroquenNote(sopranoF4)
                }
            };

            var altoF3WithDescendingSixteenthNotes = new BaroquenNote(altoF3)
            {
                Ornamentations =
                {
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoC3)
                }
            };

            var altoG3WithDescendingSixteenthNotes = new BaroquenNote(altoG3)
            {
                Ornamentations =
                {
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3)
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
                new BaroquenNote(altoG3WithDescendingSixteenthNotes),
                new BaroquenNote(sopranoC4WithAscendingSixteenthNotes),
                new BaroquenNote(altoG3WithDescendingSixteenthNotes)
            ).SetName("When sixteenth notes don't conflict, no notes are cleaned.");
        }
    }
}

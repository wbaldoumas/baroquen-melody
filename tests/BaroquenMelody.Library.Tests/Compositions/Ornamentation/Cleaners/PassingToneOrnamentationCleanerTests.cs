using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaners;

[TestFixture]
internal sealed class PassingToneOrnamentationCleanerTests
{
    private PassingToneOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new PassingToneOrnamentationCleaner();

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

            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);

            var sopranoC4WithAscendingPassingTone = new BaroquenNote(sopranoC4) { Ornamentations = { new BaroquenNote(sopranoD4) } };
            var sopranoD4WithAscendingPassingTone = new BaroquenNote(sopranoD4) { Ornamentations = { new BaroquenNote(sopranoE4) } };
            var altoF3WithDescendingPassingTone = new BaroquenNote(altoF3) { Ornamentations = { new BaroquenNote(altoE3) } };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingPassingTone)
            ).SetName("When passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoD4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoD4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingPassingTone)
            ).SetName("When passing tones don't conflict, no notes are cleaned.");
        }
    }
}

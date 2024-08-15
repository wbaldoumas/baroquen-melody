using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;

[TestFixture]
internal sealed class QuarterEighthNoteOrnamentationCleanerTests
{
    private QuarterEighthNoteOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new QuarterEighthNoteOrnamentationCleaner(Configurations.GetCompositionConfiguration());

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

            var altoG3 = new BaroquenNote(Voice.Two, Notes.G3, MusicalTimeSpan.Half);
            var altoF3 = new BaroquenNote(Voice.Two, Notes.F3, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Half);
            var altoD3 = new BaroquenNote(Voice.Two, Notes.D3, MusicalTimeSpan.Half);
            var altoC3 = new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half);

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

            var altoG3WithDescendingRun = new BaroquenNote(altoG3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoF3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3)
                }
            };

            var altoF3WithDescendingRun = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoC3)
                }
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingRun)
            ).SetName("When soprano has ascending passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When soprano has ascending passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingRun)
            ).SetName("When soprano has ascending double passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When soprano has ascending double passing tone and alto has descending sixteenth notes, then passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingRun)
            ).SetName("When passing tone and sixteenth notes don't conflict, then nothing is cleaned.");
        }
    }
}

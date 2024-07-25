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
internal sealed class MordentSixteenthNoteOrnamentationCleanerTests
{
    private MordentSixteenthNoteOrnamentationCleaner _mordentSixteenthNoteOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp() => _mordentSixteenthNoteOrnamentationCleaner = new MordentSixteenthNoteOrnamentationCleaner();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Clean_WhenCalled_CleansOrnamentation(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _mordentSixteenthNoteOrnamentationCleaner.Process(ornamentationCleaningItem);

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

            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3);
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);

            var sopranoC4WithMordent = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.Mordent,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoC4)
                }
            };

            var altoF3WithNonDissonantSixteenthNotes = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.SixteenthNoteRun,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3)
                }
            };

            var altoC3WithDissonantSixteenthNotes = new BaroquenNote(altoC3)
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
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoF3WithNonDissonantSixteenthNotes),
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoF3WithNonDissonantSixteenthNotes)
            ).SetName("When sixteenth notes are not dissonant, mordent is not cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithNonDissonantSixteenthNotes),
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoF3WithNonDissonantSixteenthNotes),
                new BaroquenNote(sopranoC4WithMordent)
            ).SetName("When sixteenth notes are not dissonant, mordent is not cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoC3WithDissonantSixteenthNotes),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoC3WithDissonantSixteenthNotes)
            ).SetName("When sixteenth notes are dissonant, mordent is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantSixteenthNotes),
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoC3WithDissonantSixteenthNotes),
                new BaroquenNote(sopranoC4)
            ).SetName("When sixteenth notes are dissonant, mordent is cleaned.");
        }
    }
}

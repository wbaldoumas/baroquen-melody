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
internal sealed class TurnAlternateTurnOrnamentationCleanerTests
{
    private TurnAlternateTurnOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new TurnAlternateTurnOrnamentationCleaner();

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
            var sopranoB3 = new BaroquenNote(Voice.Soprano, Notes.B3);
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4);
            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4);

            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3);
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3);
            var altoB3 = new BaroquenNote(Voice.Alto, Notes.B3);

            var sopranoC4WithDescendingTurn = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.Turn,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoC4),
                    new BaroquenNote(sopranoB3)
                }
            };

            var altoC3WithAscendingAlternateTurn = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.AlternateTurn,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoB3),
                    new BaroquenNote(altoC3)
                }
            };

            var altoF3WithAscendingAlternateTurn = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.AlternateTurn,
                Ornamentations =
                {
                    new BaroquenNote(altoG3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3)
                }
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithDescendingTurn),
                new BaroquenNote(altoC3WithAscendingAlternateTurn),
                new BaroquenNote(sopranoC4WithDescendingTurn),
                new BaroquenNote(altoC3)
            ).SetName("When turn notes conflict, alternate turn is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithAscendingAlternateTurn),
                new BaroquenNote(sopranoC4WithDescendingTurn),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoC4WithDescendingTurn)
            ).SetName("When turn notes conflict, alternate is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithDescendingTurn),
                new BaroquenNote(altoF3WithAscendingAlternateTurn),
                new BaroquenNote(sopranoC4WithDescendingTurn),
                new BaroquenNote(altoF3WithAscendingAlternateTurn)
            ).SetName("When turn notes don't conflict, no notes are cleaned.");
        }
    }
}

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
internal sealed class MordentEighthNoteOrnamentationCleanerTests
{
    private MordentEighthNoteOrnamentationCleaner _mordentEighthNoteOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp() => _mordentEighthNoteOrnamentationCleaner = new MordentEighthNoteOrnamentationCleaner(Configurations.GetCompositionConfiguration());

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Clean_WhenCalled_CleansOrnamentation(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _mordentEighthNoteOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);

            var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);
            var altoD3 = new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half);
            var altoC3 = new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half);

            var sopranoC4WithMordent = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.Mordent,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4),
                    new BaroquenNote(sopranoC4)
                }
            };

            var altoF3WithNonDissonantRun = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoD3),
                    new BaroquenNote(altoE3),
                    new BaroquenNote(altoF3)
                }
            };

            var altoC3WithDissonantRun = new BaroquenNote(altoC3)
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
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoF3WithNonDissonantRun),
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoF3WithNonDissonantRun)
            ).SetName("When sixteenth notes are not dissonant, mordent is not cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithNonDissonantRun),
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoF3WithNonDissonantRun),
                new BaroquenNote(sopranoC4WithMordent)
            ).SetName("When sixteenth notes are not dissonant, mordent is not cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoC3WithDissonantRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoC3WithDissonantRun)
            ).SetName("When sixteenth notes are dissonant, mordent is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithDissonantRun),
                new BaroquenNote(sopranoC4WithMordent),
                new BaroquenNote(altoC3WithDissonantRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When sixteenth notes are dissonant, mordent is cleaned.");
        }
    }
}

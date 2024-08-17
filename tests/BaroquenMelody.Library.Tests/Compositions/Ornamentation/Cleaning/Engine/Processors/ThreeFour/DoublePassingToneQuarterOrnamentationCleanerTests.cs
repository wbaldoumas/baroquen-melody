using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;

[TestFixture]
internal sealed class DoublePassingToneQuarterOrnamentationCleanerTests
{
    private DoublePassingToneQuarterOrnamentationCleaner _doublePassingToneQuarterOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _doublePassingToneQuarterOrnamentationCleaner = new DoublePassingToneQuarterOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _doublePassingToneQuarterOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoE4 = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half.Dotted(1));

            var altoD3 = new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half.Dotted(1));
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half.Dotted(1));

            var sopranoC4WithAscendingDoublePassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    },
                    new BaroquenNote(sopranoE4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            var altoD3WithAscendingPassingTone = new BaroquenNote(altoD3)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            var altoE3WithAscendingPassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoF3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoD3WithAscendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoD3WithAscendingPassingTone)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoD3WithAscendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoD3WithAscendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoE3WithAscendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoE3)
            ).SetName("When notes conflict, non-double passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithAscendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone)
            ).SetName("When notes conflict, non-double passing tone is cleaned");
        }
    }
}

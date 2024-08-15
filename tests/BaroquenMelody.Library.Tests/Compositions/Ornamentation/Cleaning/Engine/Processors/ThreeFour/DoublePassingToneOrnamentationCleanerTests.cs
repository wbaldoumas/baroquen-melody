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
internal sealed class DoublePassingToneOrnamentationCleanerTests
{
    private DoublePassingToneOrnamentationCleaner _doublePassingToneOrnamentationCleaner;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _doublePassingToneOrnamentationCleaner = new DoublePassingToneOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _doublePassingToneOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoD4 = new BaroquenNote(Voice.One, Notes.D4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoE4 = new BaroquenNote(Voice.One, Notes.E4, MusicalTimeSpan.Half.Dotted(1));

            var altoD3 = new BaroquenNote(Voice.Two, Notes.D3, MusicalTimeSpan.Half.Dotted(1));
            var altoE3 = new BaroquenNote(Voice.Two, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoF3 = new BaroquenNote(Voice.Two, Notes.F3, MusicalTimeSpan.Half.Dotted(1));
            var altoG3 = new BaroquenNote(Voice.Two, Notes.G3, MusicalTimeSpan.Half.Dotted(1));

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

            var altoG3WithDescendingDoublePassingTone = new BaroquenNote(altoG3)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoF3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    },
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            var altoF3WithDescendingDoublePassingTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    },
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Quarter
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingDoublePassingTone)
            ).SetName("When notes don't conflict, no notes are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When notes conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone)
            ).SetName("When notes conflict, lower note is cleaned");
        }
    }
}

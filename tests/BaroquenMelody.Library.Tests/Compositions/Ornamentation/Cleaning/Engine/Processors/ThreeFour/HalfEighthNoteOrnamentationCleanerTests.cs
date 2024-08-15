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
internal sealed class HalfEighthNoteOrnamentationCleanerTests
{
    private HalfEighthNoteOrnamentationCleaner _halfEighthNoteOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _halfEighthNoteOrnamentationCleaner = new HalfEighthNoteOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _halfEighthNoteOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoB3 = new BaroquenNote(Voice.Soprano, Notes.B3, MusicalTimeSpan.Half.Dotted(1));

            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3, MusicalTimeSpan.Half.Dotted(1));

            var sopranoC4WithDelayedRepeatedNote = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedRepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            var altoE3WithDelayedRepeatedNote = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedRepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            var altoD3WithDelayedRepeatedNote = new BaroquenNote(altoD3)
            {
                OrnamentationType = OrnamentationType.DelayedRepeatedNote,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            var sopranoB3WithAscendingDelayedPassingTone = new BaroquenNote(sopranoB3)
            {
                OrnamentationType = OrnamentationType.DelayedPassingTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            var altoE3WithDescendingDelayedPassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedPassingTone,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            var sopranoB3WithDelayedUpperNeighborTone = new BaroquenNote(sopranoB3)
            {
                OrnamentationType = OrnamentationType.DelayedNeighborTone,
                Ornamentations =
                {
                    new BaroquenNote(sopranoC4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            var altoE3WithDelayedLowerNeighborTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedNeighborTone,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Half + MusicalTimeSpan.Eighth
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoE3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoE3WithDelayedRepeatedNote)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoD3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoD3)
            ).SetName("When repeated notes conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoD3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoD3),
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote)
            ).SetName("When repeated notes conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoE3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoE3)
            ).SetName("When passing tones conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone)
            ).SetName("When passing tones conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone),
                new BaroquenNote(altoE3WithDelayedLowerNeighborTone),
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone),
                new BaroquenNote(altoE3)
            ).SetName("When neighbor tones conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDelayedLowerNeighborTone),
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone)
            ).SetName("When neighbor tones conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoE3WithDelayedLowerNeighborTone),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoE3)
            ).SetName("When passing tone conflicts with neighbor tone, neighbor tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDelayedLowerNeighborTone),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone)
            ).SetName("When passing tone conflicts with neighbor tone, neighbor tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoD3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoD3)
            ).SetName("When passing tone conflicts with repeated note, repeated note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoD3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone),
                new BaroquenNote(altoD3),
                new BaroquenNote(sopranoB3WithAscendingDelayedPassingTone)
            ).SetName("When passing tone conflicts with repeated note, repeated note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone),
                new BaroquenNote(altoD3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone),
                new BaroquenNote(altoD3)
            ).SetName("When neighbor tone conflicts with repeated note, repeated note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoD3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone),
                new BaroquenNote(altoD3),
                new BaroquenNote(sopranoB3WithDelayedUpperNeighborTone)
            ).SetName("When neighbor tone conflicts with repeated note, repeated note is cleaned");
        }
    }
}

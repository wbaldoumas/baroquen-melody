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
internal sealed class DelayedRunEighthOrnamentationCleanerTests
{
    private DelayedRunEighthOrnamentationCleaner _delayedRunEighthOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _delayedRunEighthOrnamentationCleaner = new DelayedRunEighthOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _delayedRunEighthOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoG4 = new BaroquenNote(Voice.Soprano, Notes.G4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoF4 = new BaroquenNote(Voice.Soprano, Notes.F4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoE4 = new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half.Dotted(1));

            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half.Dotted(1));
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3, MusicalTimeSpan.Half.Dotted(1));
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Half.Dotted(1));
            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3, MusicalTimeSpan.Half.Dotted(1));

            var sopranoC4WithAscendingDelayedRun = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedRun,
                Ornamentations =
                {
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoE4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoF4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoG4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            var sopranoG4WithDescendingRun = new BaroquenNote(sopranoG4)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(sopranoF4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoE4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(sopranoD4)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter.Dotted(1)
            };

            var altoF3WithDescendingRun = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoC3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter.Dotted(1)
            };

            var altoG3WithDescendingRun = new BaroquenNote(altoG3)
            {
                OrnamentationType = OrnamentationType.Run,
                Ornamentations =
                {
                    new BaroquenNote(altoF3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter.Dotted(1)
            };

            var altoC3WithAscendingDelayedRun = new BaroquenNote(altoC3)
            {
                OrnamentationType = OrnamentationType.DelayedRun,
                Ornamentations =
                {
                    new BaroquenNote(altoD3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoE3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoF3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
                    new BaroquenNote(altoG3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                }
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoF3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoF3WithDescendingRun)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoF3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoF3WithDescendingRun)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoG3)
            ).SetName("When notes conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingRun),
                new BaroquenNote(sopranoC4WithAscendingDelayedRun),
                new BaroquenNote(altoG3),
                new BaroquenNote(sopranoC4WithAscendingDelayedRun)
            ).SetName("When notes conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoG4WithDescendingRun),
                new BaroquenNote(altoC3WithAscendingDelayedRun),
                new BaroquenNote(sopranoG4WithDescendingRun),
                new BaroquenNote(altoC3)
            ).SetName("When notes conflict, lower note is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoC3WithAscendingDelayedRun),
                new BaroquenNote(sopranoG4WithDescendingRun),
                new BaroquenNote(altoC3),
                new BaroquenNote(sopranoG4WithDescendingRun)
            ).SetName("When notes conflict, lower note is cleaned");
        }
    }
}

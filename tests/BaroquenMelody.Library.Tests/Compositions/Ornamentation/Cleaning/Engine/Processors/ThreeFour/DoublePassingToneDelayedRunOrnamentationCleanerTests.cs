﻿using BaroquenMelody.Library.Compositions.Domain;
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
internal sealed class DoublePassingToneDelayedRunOrnamentationCleanerTests
{
    private DoublePassingToneDelayedRunOrnamentationCleaner _doublePassingToneDelayedRunOrnamentationCleaner = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2) with { DefaultNoteTimeSpan = MusicalTimeSpan.Half.Dotted(1) };

        _doublePassingToneDelayedRunOrnamentationCleaner = new DoublePassingToneDelayedRunOrnamentationCleaner(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void When_Process_is_invoked_item_is_cleaned_if_necessary(BaroquenNote noteA, BaroquenNote noteB, BaroquenNote expectedNoteA, BaroquenNote expectedNoteB)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(noteA, noteB);

        // act
        _doublePassingToneDelayedRunOrnamentationCleaner.Process(ornamentationCleaningItem);

        // assert
        noteA.Should().BeEquivalentTo(expectedNoteA);
        noteB.Should().BeEquivalentTo(expectedNoteB);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoE4 = new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half.Dotted(1));
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half.Dotted(1));

            var altoB2 = new BaroquenNote(Voice.Alto, Notes.B2, MusicalTimeSpan.Half.Dotted(1));
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half.Dotted(1));
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3, MusicalTimeSpan.Half.Dotted(1));
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half.Dotted(1));
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Half.Dotted(1));
            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3, MusicalTimeSpan.Half.Dotted(1));
            var altoA3 = new BaroquenNote(Voice.Alto, Notes.A3, MusicalTimeSpan.Half.Dotted(1));

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

            var altoA3WithDescendingDelayedRun = new BaroquenNote(altoA3)
            {
                OrnamentationType = OrnamentationType.DelayedRun,
                Ornamentations =
                {
                    new BaroquenNote(altoG3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    },
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
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            var altoG3WithDescendingDelayedRun = new BaroquenNote(altoG3)
            {
                OrnamentationType = OrnamentationType.DelayedRun,
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
                    },
                    new BaroquenNote(altoC3)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            var altoF3WithDescendingDelayedRun = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.DelayedRun,
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
                    },
                    new BaroquenNote(altoB2)
                    {
                        MusicalTimeSpan = MusicalTimeSpan.Eighth
                    }
                },
                MusicalTimeSpan = MusicalTimeSpan.Quarter
            };

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoA3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoA3WithDescendingDelayedRun)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoA3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoA3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone)
            ).SetName("When notes don't conflict, neither are cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoF3WithDescendingDelayedRun)
            ).SetName("When first note of double passing tone conflicts with first note of delayed run, double passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When first note of double passing tone conflicts with first note of delayed run, double passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoG3WithDescendingDelayedRun)
            ).SetName("When second note of double passing tone conflicts with third note of delayed run, double passing tone is cleaned");

            yield return new TestCaseData(
                new BaroquenNote(altoG3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoG3WithDescendingDelayedRun),
                new BaroquenNote(sopranoC4)
            ).SetName("When second note of double passing tone conflicts with third note of delayed run, double passing tone is cleaned");
        }
    }
}

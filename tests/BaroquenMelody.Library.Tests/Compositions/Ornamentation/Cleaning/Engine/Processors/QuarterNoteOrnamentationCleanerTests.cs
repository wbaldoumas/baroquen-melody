using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Processors;

[TestFixture]
internal sealed class QuarterNoteOrnamentationCleanerTests
{
    private QuarterNoteOrnamentationCleaner _cleaner = null!;

    [SetUp]
    public void SetUp() => _cleaner = new QuarterNoteOrnamentationCleaner(Configurations.GetCompositionConfiguration());

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
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half);
            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half);
            var sopranoE4 = new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half);

            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half);
            var altoD3 = new BaroquenNote(Voice.Alto, Notes.D3, MusicalTimeSpan.Half);
            var altoC3 = new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half);

            var sopranoC4WithAscendingPassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations = { new BaroquenNote(sopranoD4) }
            };

            var sopranoC4WithUpperDelayedNeighborTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedNeighborTone,
                Ornamentations = { new BaroquenNote(sopranoD4) }
            };

            var sopranoC4WithUpperNeighborTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.NeighborTone,
                Ornamentations = { new BaroquenNote(sopranoD4) }
            };

            var sopranoC4WithRepeatedNote = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.RepeatedNote,
                Ornamentations = { new BaroquenNote(sopranoC4) }
            };

            var sopranoC4WithDelayedRepeatedNote = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedRepeatedNote,
                Ornamentations = { new BaroquenNote(sopranoC4) }
            };

            var sopranoC4WithAscendingDelayedPassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedPassingTone,
                Ornamentations = { new BaroquenNote(sopranoD4) }
            };

            var sopranoC4WithAscendingDoublePassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations = { new BaroquenNote(sopranoD4), new BaroquenNote(sopranoE4) }
            };

            var sopranoC4WithAscendingDelayedDoublePassingTone = new BaroquenNote(sopranoC4)
            {
                OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
                Ornamentations = { new BaroquenNote(sopranoD4), new BaroquenNote(sopranoE4) }
            };

            var sopranoD4WithAscendingPassingTone = new BaroquenNote(sopranoD4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations = { new BaroquenNote(sopranoE4) }
            };

            var altoF3WithDescendingPassingTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations = { new BaroquenNote(altoE3) }
            };

            var altoF3WithDescendingDelayedPassingTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.DelayedPassingTone,
                Ornamentations = { new BaroquenNote(altoE3) }
            };

            var altoF3WithDescendingDoublePassingTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations = { new BaroquenNote(altoE3), new BaroquenNote(altoD3) }
            };

            var altoF3WithDescendingDelayedDoublePassingTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
                Ornamentations = { new BaroquenNote(altoE3), new BaroquenNote(altoD3) }
            };

            var altoF3WithUnknownOrnamentation = new BaroquenNote(altoF3)
            {
                OrnamentationType = (OrnamentationType)byte.MaxValue,
                Ornamentations = { new BaroquenNote(altoE3) }
            };

            var altoE3WithDescendingPassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations = { new BaroquenNote(altoD3) }
            };

            var altoE3WithLowerNeighborTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.NeighborTone,
                Ornamentations = { new BaroquenNote(altoD3) }
            };

            var altoE3WithDescendingDoublePassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DoublePassingTone,
                Ornamentations = { new BaroquenNote(altoD3), new BaroquenNote(altoC3) }
            };

            var altoE3WithDescendingDelayedPassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedPassingTone,
                Ornamentations = { new BaroquenNote(altoD3) }
            };

            var altoE3WithDescendingDelayedDoublePassingTone = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
                Ornamentations = { new BaroquenNote(altoD3), new BaroquenNote(altoC3) }
            };

            var altoE3WithDelayedRepeatedNote = new BaroquenNote(altoE3)
            {
                OrnamentationType = OrnamentationType.DelayedRepeatedNote,
                Ornamentations = { new BaroquenNote(altoE3) }
            };

            var altoF3WithLowerDelayedNeighborTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.DelayedNeighborTone,
                Ornamentations = { new BaroquenNote(altoE3) }
            };

            var altoF3WithLowerNeighborTone = new BaroquenNote(altoF3)
            {
                OrnamentationType = OrnamentationType.NeighborTone,
                Ornamentations = { new BaroquenNote(altoE3) }
            };

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
                new BaroquenNote(sopranoC4WithAscendingDelayedPassingTone),
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDelayedPassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When delayed passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDelayedPassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingDelayedPassingTone)
            ).SetName("When delayed passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When double passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone)
            ).SetName("When double passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When delayed double passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone)
            ).SetName("When delayed double passing tones conflict, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When double passing tone conflicts with passing tone, passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingDoublePassingTone)
            ).SetName("When double passing tone conflicts with passing tone, passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When delayed double passing tone conflicts with delayed passing tone, delayed passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingDelayedDoublePassingTone)
            ).SetName("When delayed double passing tone conflicts with delayed passing tone, delayed passing tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithDescendingPassingTone)
            ).SetName("When repeated eighth conflicts with passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated eighth conflicts with passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithDescendingDoublePassingTone)
            ).SetName("When repeated eighth conflicts with double passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated eighth conflicts with double passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithDescendingDelayedPassingTone)
            ).SetName("When repeated eighth conflicts with delayed passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated eighth conflicts with delayed passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone)
            ).SetName("When repeated eighth conflicts with delayed double passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4WithDelayedRepeatedNote),
                new BaroquenNote(altoE3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When repeated eighth conflicts with delayed double passing tone, repeated eighth is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone)
            ).SetName("When delayed neighbor tone conflicts with delayed passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3WithDescendingDelayedPassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When delayed neighbor tone conflicts with delayed passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoF3WithDescendingDelayedDoublePassingTone)
            ).SetName("When delayed neighbor tone conflicts with delayed double passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3WithDescendingDelayedDoublePassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When delayed neighbor tone conflicts with delayed double passing tone, neighbor tone is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoE3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoE3)
            ).SetName("When delayed neighbor tone conflicts with repeated dotted eighth sixteenth, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithDelayedRepeatedNote),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoE3),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone)
            ).SetName("When delayed neighbor tone conflicts with repeated dotted eighth sixteenth, repeated note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3WithLowerDelayedNeighborTone),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3)
            ).SetName("When delayed neighbor tone conflicts with neighbor tone, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3WithLowerDelayedNeighborTone),
                new BaroquenNote(sopranoC4WithUpperDelayedNeighborTone),
                new BaroquenNote(altoF3)
            ).SetName("When delayed neighbor tone conflicts with delayed neighbor tone, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3)
            ).SetName("When neighbor tone conflicts with neighbor tone, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3)
            ).SetName("When neighbor tone conflicts with neighbor tone, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoF3WithDescendingPassingTone)
            ).SetName("When neighbor tone conflicts with passing tone, neighbor tone note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When neighbor tone conflicts with passing tone, neighbor tone note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoF3WithDescendingDoublePassingTone)
            ).SetName("When neighbor tone conflicts with double passing tone, neighbor tone note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4WithUpperNeighborTone),
                new BaroquenNote(altoF3WithDescendingDoublePassingTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When neighbor tone conflicts with double passing tone, neighbor tone note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4),
                new BaroquenNote(altoE3WithLowerNeighborTone)
            ).SetName("When neighbor tone conflicts with repeated eighth, repeated eighth note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4WithRepeatedNote),
                new BaroquenNote(altoE3WithLowerNeighborTone),
                new BaroquenNote(sopranoC4)
            ).SetName("When neighbor tone conflicts with repeated eighth, repeated eighth note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(altoF3WithUnknownOrnamentation),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3),
                new BaroquenNote(sopranoC4WithAscendingPassingTone)
            ).SetName("When unknown ornamentation, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithUnknownOrnamentation),
                new BaroquenNote(sopranoC4WithAscendingPassingTone),
                new BaroquenNote(altoF3)
            ).SetName("When unknown ornamentation, lower note is cleaned.");

            yield return new TestCaseData(
                new BaroquenNote(sopranoD4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingPassingTone),
                new BaroquenNote(sopranoD4WithAscendingPassingTone),
                new BaroquenNote(altoF3WithDescendingPassingTone)
            ).SetName("When passing tones don't conflict, no notes are cleaned.");
        }
    }
}

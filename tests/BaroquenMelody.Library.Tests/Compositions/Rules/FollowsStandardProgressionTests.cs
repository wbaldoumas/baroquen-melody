using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class FollowsStandardProgressionTests
{
    private FollowsStandardProgression _followsStandardProgression = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.G6),
                new(Voice.Alto, Notes.C2, Notes.G5),
                new(Voice.Tenor, Notes.C1, Notes.G4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _followsStandardProgression = new FollowsStandardProgression(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult) =>
        _followsStandardProgression.Evaluate(precedingChords, nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Half);

            var i = new BaroquenChord([sopranoC4, altoE3, tenorG2]);

            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3, MusicalTimeSpan.Half);
            var tenorA2 = new BaroquenNote(Voice.Tenor, Notes.A2, MusicalTimeSpan.Half);

            var ii = new BaroquenChord([sopranoD4, altoF3, tenorA2]);

            var sopranoE4 = new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half);
            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3, MusicalTimeSpan.Half);
            var tenorB2 = new BaroquenNote(Voice.Tenor, Notes.B2, MusicalTimeSpan.Half);

            var iii = new BaroquenChord([sopranoE4, altoG3, tenorB2]);

            var sopranoF4 = new BaroquenNote(Voice.Soprano, Notes.F4, MusicalTimeSpan.Half);
            var altoA3 = new BaroquenNote(Voice.Alto, Notes.A3, MusicalTimeSpan.Half);
            var tenorC3 = new BaroquenNote(Voice.Tenor, Notes.C3, MusicalTimeSpan.Half);

            var iv = new BaroquenChord([sopranoF4, altoA3, tenorC3]);

            var sopranoG4 = new BaroquenNote(Voice.Soprano, Notes.G4, MusicalTimeSpan.Half);
            var altoB3 = new BaroquenNote(Voice.Alto, Notes.B3, MusicalTimeSpan.Half);
            var tenorD3 = new BaroquenNote(Voice.Tenor, Notes.D3, MusicalTimeSpan.Half);

            var v = new BaroquenChord([sopranoG4, altoB3, tenorD3]);

            var sopranoA4 = new BaroquenNote(Voice.Soprano, Notes.A4, MusicalTimeSpan.Half);
            var altoC4 = new BaroquenNote(Voice.Alto, Notes.C4, MusicalTimeSpan.Half);
            var tenorE3 = new BaroquenNote(Voice.Tenor, Notes.E3, MusicalTimeSpan.Half);

            var vi = new BaroquenChord([sopranoA4, altoC4, tenorE3]);

            var sopranoB4 = new BaroquenNote(Voice.Soprano, Notes.B4, MusicalTimeSpan.Half);
            var altoD4 = new BaroquenNote(Voice.Alto, Notes.D4, MusicalTimeSpan.Half);
            var tenorF3 = new BaroquenNote(Voice.Tenor, Notes.F3, MusicalTimeSpan.Half);

            var vii = new BaroquenChord([sopranoB4, altoD4, tenorF3]);

            yield return new TestCaseData(new List<BaroquenChord>(), i, true).SetName("First chord is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, i, true).SetName("I -> I is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, ii, true).SetName("I -> II is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, iii, true).SetName("I -> III is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, iv, true).SetName("I -> IV is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, v, true).SetName("I -> V is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, vi, true).SetName("I -> VI is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { i }, vii, true).SetName("I -> VII is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, i, true).SetName("II -> I is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, ii, true).SetName("II -> II is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, iii, true).SetName("II -> III is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, v, true).SetName("II -> V is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, vi, true).SetName("II -> VI is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, vii, true).SetName("II -> VII is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, ii, true).SetName("III -> II is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, iii, true).SetName("III -> III is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, iv, true).SetName("III -> IV is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, vi, true).SetName("III -> VI is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, i, true).SetName("IV -> I is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, iii, true).SetName("IV -> III is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, iv, true).SetName("IV -> IV is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, v, true).SetName("IV -> V is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, vii, true).SetName("IV -> VII is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, i, true).SetName("V -> I is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, v, true).SetName("V -> V is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, vi, true).SetName("V -> VI is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, ii, true).SetName("VI -> II is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, iv, true).SetName("VI -> IV is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, vi, true).SetName("VI -> VI is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, i, true).SetName("VII -> I is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, iii, true).SetName("VII -> III is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, vi, true).SetName("VII -> VI is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, vii, true).SetName("VII -> VII is always valid.");

            yield return new TestCaseData(new List<BaroquenChord> { ii }, iv, false).SetName("II -> IV is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, i, false).SetName("III -> I is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, v, false).SetName("III -> V is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { iii }, vii, false).SetName("III -> VII is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, ii, false).SetName("IV -> II is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { iv }, vi, false).SetName("IV -> VI is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, ii, false).SetName("V -> II is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, iii, false).SetName("V -> III is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, iv, false).SetName("V -> IV is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { v }, vii, false).SetName("V -> VII is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, i, false).SetName("VI -> I is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, iii, false).SetName("VI -> III is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, v, false).SetName("VI -> V is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vi }, vii, false).SetName("VI -> VII is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, ii, false).SetName("VII -> II is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, iv, false).SetName("VII -> IV is invalid.");

            yield return new TestCaseData(new List<BaroquenChord> { vii }, v, false).SetName("VII -> V is invalid.");
        }
    }
}

using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations.Rules;

[TestFixture]
internal sealed class AvoidDissonantLeapsTests
{
    private AvoidDissonantLeaps _avoidDissonantLeaps = null!;

    [SetUp]
    public void SetUp()
    {
        var phrasingConfiguration = new PhrasingConfiguration(
            PhraseLengths: [2, 4, 8],
            MaxPhraseRepetitions: 4,
            MinPhraseRepetitionPoolSize: 10,
            PhraseRepetitionProbability: 90
        );

        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.G5, Notes.G6),
                new(Voice.Alto, Notes.C4, Notes.G5),
                new(Voice.Tenor, Notes.C3, Notes.C4),
                new(Voice.Bass, Notes.C2, Notes.C3)
            },
            phrasingConfiguration,
            Scale.Parse("C Major"),
            Meter.FourFour,
            100
        );

        _avoidDissonantLeaps = new AvoidDissonantLeaps(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        var result = _avoidDissonantLeaps.Evaluate(precedingChords, nextChord);

        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoB3 = new BaroquenNote(Voice.Soprano, Notes.B3);
            var altoE4 = new BaroquenNote(Voice.Alto, Notes.E4);
            var tenorG3 = new BaroquenNote(Voice.Tenor, Notes.G3);
            var baseE2 = new BaroquenNote(Voice.Bass, Notes.E2);

            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4);
            var altoF4 = new BaroquenNote(Voice.Alto, Notes.F4);
            var tenorA3 = new BaroquenNote(Voice.Tenor, Notes.A3);
            var bassF2 = new BaroquenNote(Voice.Bass, Notes.F2);

            var sopranoF4 = new BaroquenNote(Voice.Soprano, Notes.F4);
            var altoA4 = new BaroquenNote(Voice.Alto, Notes.A4);
            var tenorC4 = new BaroquenNote(Voice.Tenor, Notes.C4);
            var bassA2 = new BaroquenNote(Voice.Bass, Notes.A2);

            var eMinor = new BaroquenChord([sopranoB3, altoE4, tenorG3, baseE2]);
            var fMajor = new BaroquenChord([sopranoC4, altoF4, tenorA3, bassF2]);
            var fMajorWithLeap = new BaroquenChord([sopranoF4, altoA4, tenorC4, bassA2]);

            yield return new TestCaseData(new List<BaroquenChord>(), eMinor, true).SetName("No preceding chords is non-dissonant.");

            yield return new TestCaseData(new List<BaroquenChord> { fMajor }, fMajor, true).SetName("Oblique motion is non-dissonant.");

            yield return new TestCaseData(new List<BaroquenChord> { eMinor }, fMajor, true).SetName("Consonant leap is non-dissonant.");

            yield return new TestCaseData(new List<BaroquenChord> { eMinor }, fMajorWithLeap, false).SetName("Dissonant leap is dissonant.");
        }
    }
}

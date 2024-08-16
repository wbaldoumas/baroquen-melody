using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class AvoidDissonantLeapsTests
{
    private AvoidDissonantLeaps _avoidDissonantLeaps = null!;

    [SetUp]
    public void SetUp() => _avoidDissonantLeaps = new AvoidDissonantLeaps(Configurations.GetCompositionConfiguration());

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
            var sopranoB3 = new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Half);
            var altoE4 = new BaroquenNote(Instrument.Two, Notes.E4, MusicalTimeSpan.Half);
            var tenorG3 = new BaroquenNote(Instrument.Three, Notes.G3, MusicalTimeSpan.Half);
            var baseE2 = new BaroquenNote(Instrument.Four, Notes.E2, MusicalTimeSpan.Half);

            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var altoF4 = new BaroquenNote(Instrument.Two, Notes.F4, MusicalTimeSpan.Half);
            var tenorA3 = new BaroquenNote(Instrument.Three, Notes.A3, MusicalTimeSpan.Half);
            var bassF2 = new BaroquenNote(Instrument.Four, Notes.F2, MusicalTimeSpan.Half);

            var sopranoF4 = new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half);
            var altoA4 = new BaroquenNote(Instrument.Two, Notes.A4, MusicalTimeSpan.Half);
            var tenorC4 = new BaroquenNote(Instrument.Three, Notes.C4, MusicalTimeSpan.Half);
            var bassA2 = new BaroquenNote(Instrument.Four, Notes.A2, MusicalTimeSpan.Half);

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

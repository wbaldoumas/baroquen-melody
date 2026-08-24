using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules.Melodic;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules.Melodic;

[TestFixture]
internal sealed class HandleAscendingSeventhTests
{
    private MelodicCompositionRuleAdapter _handleAscendingSeventh = null!;

    // The chord-level cases predate the melodic viewpoint; evaluating through the adapter keeps them verbatim while
    // covering the rule and its per-voice aggregation together.
    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _handleAscendingSeventh = new MelodicCompositionRuleAdapter(new HandleAscendingSeventh(compositionConfiguration));
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        // act
        var result = _handleAscendingSeventh.Evaluate(precedingChords, nextChord);

        // assert
        result.Should().Be(expectedResult);
    }

    // Audit: rules-2 - only a half-step leading tone has an upward tendency; the whole-step subtonic of a modal scale
    // (A Aeolian's G) is free to fall, so F4 -> G4 -> E4 must be accepted.
    [Test]
    public void Evaluate_AllowsTheSubtonicToDescend_InAMinorModeWithoutALeadingTone()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2, tonic: NoteName.A, mode: Mode.Aeolian);
        var handleAscendingSeventh = new MelodicCompositionRuleAdapter(new HandleAscendingSeventh(compositionConfiguration));

        var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);
        var precedingChords = new List<BaroquenChord>
        {
            new([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half), altoE3]),
            new([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half), altoE3])
        };
        var nextChord = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half), altoE3]);

        // act
        var result = handleAscendingSeventh.Evaluate(precedingChords, nextChord);

        // assert
        result.Should().BeTrue("the subtonic of A Aeolian is a whole step below the tonic and carries no upward tendency");
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoA3 = new BaroquenNote(Instrument.One, Notes.A3, MusicalTimeSpan.Half);
            var sopranoB3 = new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Half);
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);

            var cMajor = new BaroquenChord([sopranoC4, altoE3]);
            var eMinor = new BaroquenChord([sopranoB3, altoE3]);
            var aMinor = new BaroquenChord([sopranoA3, altoE3]);

            yield return new TestCaseData(new List<BaroquenChord>(), cMajor, true).SetName("No need to handle ascending seventh if it's the first chord.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor }, eMinor, true).SetName("No need to handle ascending seventh if it's the second chord.");

            yield return new TestCaseData(new List<BaroquenChord> { aMinor, eMinor }, cMajor, true).SetName("Correctly handles ascending seventh.");

            yield return new TestCaseData(new List<BaroquenChord> { aMinor, eMinor }, aMinor, false).SetName("Incorrectly handles ascending seventh.");

            yield return new TestCaseData(new List<BaroquenChord> { cMajor, eMinor }, aMinor, true).SetName("No need to handle ascending seventh if its descending.");

            // The pre-viewpoint rule threw a KeyNotFoundException for these voice-set mismatches; the melodic
            // projection skips a voice without its full context instead.
            yield return new TestCaseData(new List<BaroquenChord> { new BaroquenChord([altoE3]), eMinor }, cMajor, true).SetName("A leading tone whose earlier context chord lacks the voice is not evaluated.");

            yield return new TestCaseData(new List<BaroquenChord> { aMinor, eMinor }, new BaroquenChord([altoE3]), true).SetName("A leading-tone voice absent from the next chord is not evaluated.");
        }
    }
}

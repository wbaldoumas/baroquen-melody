using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsIntervalWithinVoiceRangeTests
{
    private IsIntervalWithinVoiceRange _isIntervalWithinVoiceRange;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration();

        _isIntervalWithinVoiceRange = new IsIntervalWithinVoiceRange(compositionConfiguration, interval: 5);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem ornamentationItem, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var result = _isIntervalWithinVoiceRange.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedInputPolicyResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var testCompositionContext = new FixedSizeList<Beat>(1);

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.C5, MusicalTimeSpan.Half), new BaroquenNote(Voice.Two, Notes.G4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName($"When added interval is within voice range, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.C6, MusicalTimeSpan.Half), new BaroquenNote(Voice.Two, Notes.G3, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName($"When added interval is not within voice range, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

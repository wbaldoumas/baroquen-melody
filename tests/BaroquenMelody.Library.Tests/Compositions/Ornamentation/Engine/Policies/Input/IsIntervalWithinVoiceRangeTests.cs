using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
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
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C5, Notes.G6),
                new(Voice.Alto, Notes.G3, Notes.C5),
                new(Voice.Tenor, Notes.C2, Notes.G3),
                new(Voice.Bass, Notes.G0, Notes.C2)
            },
            PhrasingConfiguration.Default,
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            25
        );

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
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C5), new BaroquenNote(Voice.Alto, Notes.G4)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName($"When added interval is within voice range, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C6), new BaroquenNote(Voice.Alto, Notes.G3)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName($"When added interval is not within voice range, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

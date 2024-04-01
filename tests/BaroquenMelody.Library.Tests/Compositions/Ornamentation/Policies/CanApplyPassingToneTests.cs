using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Policies;

[TestFixture]
internal sealed class CanApplyPassingToneTests
{
    private CanApplyPassingTone _canApplyPassingTone = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.A4, Notes.A5),
                new(Voice.Alto, Notes.C3, Notes.C4)
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _canApplyPassingTone = new CanApplyPassingTone(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem item, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var inputPolicyResult = _canApplyPassingTone.ShouldProcess(item);

        // assert
        inputPolicyResult.Should().Be(expectedInputPolicyResult);
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
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.F4)]))
                ),
                InputPolicyResult.Continue
            ).SetName($"When notes are a third apart, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4)]))
                ),
                InputPolicyResult.Reject
            ).SetName($"When notes are not a third apart, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

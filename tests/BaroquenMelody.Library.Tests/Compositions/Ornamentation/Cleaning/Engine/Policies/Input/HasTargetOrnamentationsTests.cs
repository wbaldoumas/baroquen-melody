using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;

[TestFixture]
internal sealed class HasTargetOrnamentationsTests
{
    [Test]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.PassingTone, InputPolicyResult.Continue)]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, InputPolicyResult.Continue)]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, InputPolicyResult.Continue)]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, InputPolicyResult.Reject)]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, InputPolicyResult.Reject)]
    public void ShouldProcess_WhenCalled_ReturnsExpectedResult(
        OrnamentationType targetOrnamentationType,
        OrnamentationType otherTargetOrnamentationType,
        OrnamentationType noteOrnamentationType,
        OrnamentationType otherNoteOrnamentationType,
        InputPolicyResult expectedResult)
    {
        // arrange
        var item = new OrnamentationCleaningItem(
            new BaroquenNote(Voice.Soprano, Notes.C4) { OrnamentationType = noteOrnamentationType },
            new BaroquenNote(Voice.Alto, Notes.C3) { OrnamentationType = otherNoteOrnamentationType }
        );

        var policy = new HasTargetOrnamentations(targetOrnamentationType, otherTargetOrnamentationType);

        // act
        var result = policy.ShouldProcess(item);

        // assert
        result.Should().Be(expectedResult);
    }
}

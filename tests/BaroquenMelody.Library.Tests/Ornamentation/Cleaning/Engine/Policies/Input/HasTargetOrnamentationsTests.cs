using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Cleaning.Engine.Policies.Input;

[TestFixture]
internal sealed class HasTargetOrnamentationsTests
{
    [Test]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.PassingTone, OrnamentationType.PassingTone, InputPolicyResult.Continue)]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, InputPolicyResult.Reject)]
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
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { OrnamentationType = noteOrnamentationType },
            new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half) { OrnamentationType = otherNoteOrnamentationType }
        );

        var policy = new HasTargetOrnamentations(targetOrnamentationType, otherTargetOrnamentationType);

        // act
        var result = policy.ShouldProcess(item);

        // assert
        result.Should().Be(expectedResult);
    }
}

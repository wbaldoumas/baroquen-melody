using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class BeatContainsTargetOrnamentationTests
{
    private BeatContainsTargetOrnamentation _beatContainsTargetOrnamentation = null!;

    [SetUp]
    public void SetUp() => _beatContainsTargetOrnamentation = new BeatContainsTargetOrnamentation(OrnamentationType.PassingTone);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem ornamentationItem, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var result = _beatContainsTargetOrnamentation.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedInputPolicyResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var testCompositionContext = new FixedSizeList<Beat>(1);

            var noteWithPassingTone = new BaroquenNote(Voice.Soprano, Notes.A4)
            {
                OrnamentationType = OrnamentationType.PassingTone,
                Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.G4) }
            };

            var noteWithDelayedPassingTone = new BaroquenNote(Voice.Soprano, Notes.A4)
            {
                OrnamentationType = OrnamentationType.DelayedPassingTone,
                Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.G4) }
            };

            var noteWithoutOrnamentation = new BaroquenNote(
                Voice.Soprano,
                Notes.A4
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([noteWithPassingTone])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName($"When note has passing tone, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([noteWithDelayedPassingTone])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName($"When note has delayed passing tone, then {nameof(InputPolicyResult.Reject)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([noteWithoutOrnamentation])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName($"When note has no ornamentation, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

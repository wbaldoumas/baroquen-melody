using Atrea.PolicyEngine.Policies.Input;
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
internal sealed class NextBeatHasTargetNotesTests
{
    private NextBeatHasTargetNotes _nextBeatHasTargetNotes;

    [SetUp]
    public void SetUp() => _nextBeatHasTargetNotes = new NextBeatHasTargetNotes([NoteName.G, NoteName.B, NoteName.D]);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem ornamentationItem, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var result = _nextBeatHasTargetNotes.ShouldProcess(ornamentationItem);

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
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4), new BaroquenNote(Voice.Alto, Notes.B4), new BaroquenNote(Voice.Bass, Notes.D4)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4), new BaroquenNote(Voice.Alto, Notes.B4), new BaroquenNote(Voice.Bass, Notes.D4)]))
                ),
                InputPolicyResult.Continue
            ).SetName($"When notes are G, B, and D, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4), new BaroquenNote(Voice.Alto, Notes.B4), new BaroquenNote(Voice.Tenor, Notes.E4)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4), new BaroquenNote(Voice.Alto, Notes.B4), new BaroquenNote(Voice.Tenor, Notes.E4)]))
                ),
                InputPolicyResult.Reject
            ).SetName($"When notes are G, B, and E, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

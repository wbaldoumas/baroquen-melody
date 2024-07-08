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
internal sealed class NoteIsTargetNoteTests
{
    private NoteIsTargetNote _noteIsTargetNote = null!;

    [SetUp]
    public void SetUp() => _noteIsTargetNote = new NoteIsTargetNote(NoteName.A);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem ornamentationItem, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var result = _noteIsTargetNote.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedInputPolicyResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var testCompositionContext = new FixedSizeList<Beat>(1);

            var sopranoNoteWithTargetNoteName = new BaroquenNote(Voice.Soprano, Notes.A4);
            var altoNoteWithTargetNoteName = new BaroquenNote(Voice.Alto, Notes.A4);
            var sopranoNoteWithoutNoteName = new BaroquenNote(Voice.Soprano, Notes.G4);
            var altoNoteWithoutNoteName = new BaroquenNote(Voice.Alto, Notes.G4);

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([sopranoNoteWithTargetNoteName, altoNoteWithoutNoteName])),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    testCompositionContext,
                    new Beat(new BaroquenChord([sopranoNoteWithoutNoteName, altoNoteWithTargetNoteName])),
                    null
                ),
                InputPolicyResult.Reject
            );
        }
    }
}

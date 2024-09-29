using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsTargetNoteTests
{
    private IsTargetNote _isTargetNote = null!;

    [SetUp]
    public void SetUp() => _isTargetNote = new IsTargetNote(NoteName.A);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem ornamentationItem, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var result = _isTargetNote.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedInputPolicyResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var testCompositionContext = new FixedSizeList<Beat>(1);

            var sopranoNoteWithTargetNoteName = new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half);
            var altoNoteWithTargetNoteName = new BaroquenNote(Instrument.Two, Notes.A4, MusicalTimeSpan.Half);
            var sopranoNoteWithoutNoteName = new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half);
            var altoNoteWithoutNoteName = new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Half);

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([sopranoNoteWithTargetNoteName, altoNoteWithoutNoteName])),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([sopranoNoteWithoutNoteName, altoNoteWithTargetNoteName])),
                    null
                ),
                InputPolicyResult.Reject
            );
        }
    }
}

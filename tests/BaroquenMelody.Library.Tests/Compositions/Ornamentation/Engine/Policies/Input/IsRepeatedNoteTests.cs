using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsRepeatedNoteTests
{
    private IsRepeatedNote _isRepeatedNote = null!;

    [SetUp]
    public void SetUp() => _isRepeatedNote = new IsRepeatedNote();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess_ShouldReturnExpectedResult(OrnamentationItem ornamentationItem, InputPolicyResult expectedResult)
    {
        // act
        var result = _isRepeatedNote.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Continue
            ).SetName("When notes are repeated, policy continues.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName("When notes are not repeated, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Tenor,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName("When voice is not present, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When next beat is null, policy rejects.");
        }
    }
}

using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

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
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.B4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Four, Notes.D4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.B4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Four, Notes.D4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Continue
            ).SetName($"When notes are G, B, and D, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.B4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Three, Notes.E4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.B4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Three, Notes.E4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName($"When notes are G, B, and E, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

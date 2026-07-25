using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class LeaningToneIsNotRestruckTests
{
    private LeaningToneIsNotRestruck _leaningToneIsNotRestruck = null!;

    [SetUp]
    public void SetUp() => _leaningToneIsNotRestruck = new LeaningToneIsNotRestruck(TestCompositionConfigurations.Get(2));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess_ShouldReturnExpectedResult(OrnamentationItem ornamentationItem, InputPolicyResult expectedResult)
    {
        // act
        var result = _leaningToneIsNotRestruck.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            // the leaning tone above C4 is D4: approaching from E4 is the classic filled descending third
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    PrecedingBeats(new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName("When the approach is a descending third fill, policy continues.");

            // the voice just sounded D4 itself: striking D4 again as the leaning tone is a re-struck
            // preparation, which is the suspension applicator's territory
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    PrecedingBeats(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When the voice just sounded the leaning tone, policy rejects.");

            // an octave-displaced D3 is not a re-strike of D4
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    PrecedingBeats(new BaroquenNote(Instrument.One, Notes.D3, MusicalTimeSpan.Half)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName("When the preceding note is the leaning tone's pitch class in another octave, policy continues.");

            // the preceding beat's final sounded pitch is its last ornamentation sub-note, not its principal
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    PrecedingBeats(OrnamentedNote(Notes.F4, lastSoundedNote: Notes.D4)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When the preceding beat's ornamentation ends on the leaning tone, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName("When there is no preceding beat, policy continues.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    PrecedingBeats(new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName("When the preceding beat lacks the instrument, policy continues.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Two,
                    PrecedingBeats(new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When the instrument is not present in the beat, policy rejects.");

            // C#4 is not in the C major scale, so the leaning tone cannot be classified
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    PrecedingBeats(new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When the target note is not a scale note, policy rejects.");
        }
    }

    private static FixedSizeList<Beat> PrecedingBeats(BaroquenNote precedingNote)
    {
        var precedingBeats = new FixedSizeList<Beat>(1);

        precedingBeats.Add(new Beat(new BaroquenChord([precedingNote])));

        return precedingBeats;
    }

    private static BaroquenNote OrnamentedNote(Note principal, Note lastSoundedNote)
    {
        var note = new BaroquenNote(Instrument.One, principal, MusicalTimeSpan.Half);

        note.Ornamentations.Add(new BaroquenNote(Instrument.One, lastSoundedNote, MusicalTimeSpan.Quarter));

        return note;
    }
}

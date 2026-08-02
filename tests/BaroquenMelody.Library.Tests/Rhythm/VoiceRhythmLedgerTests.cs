using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rhythm;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rhythm;

[TestFixture]
internal sealed class VoiceRhythmLedgerTests
{
    [Test]
    public void RecordedNotes_AreFoundByReference_AndUnrecordedNotesAreNot()
    {
        // arrange
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var heldNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var floridNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);
        var unrecordedNote = new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half);

        // act
        voiceRhythmLedger.RecordHeldNote(heldNote);
        voiceRhythmLedger.RecordFloridNote(floridNote);

        // assert
        voiceRhythmLedger.IsHeldNote(heldNote).Should().BeTrue();
        voiceRhythmLedger.IsFloridNote(floridNote).Should().BeTrue();
        voiceRhythmLedger.IsHeldNote(floridNote).Should().BeFalse();
        voiceRhythmLedger.IsFloridNote(heldNote).Should().BeFalse();
        voiceRhythmLedger.IsHeldNote(unrecordedNote).Should().BeFalse();
        voiceRhythmLedger.IsFloridNote(unrecordedNote).Should().BeFalse();
    }

    [Test]
    public void ValueEqualNotes_DoNotCollide()
    {
        // arrange - equal-pitch repeats are the ledger's main population: a deep copy of a recorded note
        // (the phraser's and exposition's idiom) must NOT inherit its role
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var recordedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var valueEqualCopy = new BaroquenNote(recordedNote);

        // act
        voiceRhythmLedger.RecordHeldNote(recordedNote);

        // assert
        recordedNote.Equals(valueEqualCopy).Should().BeTrue("the copy is value-equal by construction");
        voiceRhythmLedger.IsHeldNote(valueEqualCopy).Should().BeFalse("roles attach to the emitted instance, never to copies");
    }

    [Test]
    public void RecordingTheSameNoteTwice_IsIdempotent()
    {
        // arrange
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        // act
        voiceRhythmLedger.RecordHeldNote(note);
        voiceRhythmLedger.RecordHeldNote(note);

        // assert
        voiceRhythmLedger.IsHeldNote(note).Should().BeTrue();
    }

    [Test]
    public void Clear_ForgetsEveryRecordedNote()
    {
        // arrange
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var heldNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var floridNote = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);

        voiceRhythmLedger.RecordHeldNote(heldNote);
        voiceRhythmLedger.RecordFloridNote(floridNote);

        // act
        voiceRhythmLedger.Clear();

        // assert
        voiceRhythmLedger.IsHeldNote(heldNote).Should().BeFalse();
        voiceRhythmLedger.IsFloridNote(floridNote).Should().BeFalse();
    }
}

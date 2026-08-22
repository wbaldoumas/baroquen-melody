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
        var escalatedNote = new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half);
        var figurationNote = new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half);
        var padNote = new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half);

        voiceRhythmLedger.RecordHeldNote(heldNote);
        voiceRhythmLedger.RecordFloridNote(floridNote);
        voiceRhythmLedger.RecordDivisionIntensity(escalatedNote, 140);
        voiceRhythmLedger.RecordTextureFigurationNote(figurationNote);
        voiceRhythmLedger.RecordTexturePadNote(padNote);

        // act
        voiceRhythmLedger.Clear();

        // assert
        voiceRhythmLedger.IsHeldNote(heldNote).Should().BeFalse();
        voiceRhythmLedger.IsFloridNote(floridNote).Should().BeFalse();
        voiceRhythmLedger.TryGetDivisionIntensity(escalatedNote, out _).Should().BeFalse();
        voiceRhythmLedger.IsTextureFigurationNote(figurationNote).Should().BeFalse();
        voiceRhythmLedger.IsTexturePadNote(padNote).Should().BeFalse();
    }

    [Test]
    public void TextureFigurationNotes_AttachToTheInstance_NeverToValueEqualCopies()
    {
        // arrange - the same reference-identity contract as the other stores: a deep copy carries no mark
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var recordedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var valueEqualCopy = new BaroquenNote(recordedNote);

        // act
        voiceRhythmLedger.RecordTextureFigurationNote(recordedNote);

        // assert
        voiceRhythmLedger.IsTextureFigurationNote(recordedNote).Should().BeTrue();
        voiceRhythmLedger.IsTextureFigurationNote(valueEqualCopy).Should().BeFalse("texture marks attach to the emitted instance, never to copies");
        voiceRhythmLedger.IsHeldNote(recordedNote).Should().BeFalse("the texture store is orthogonal to the held store");
        voiceRhythmLedger.IsFloridNote(recordedNote).Should().BeFalse("the texture store is orthogonal to the florid store");
    }

    [Test]
    public void TexturePadNotes_AttachToTheInstance_NeverToValueEqualCopies()
    {
        // arrange - the same reference-identity contract as the other stores: a deep copy carries no mark
        // until the restatement pass re-records it, and pad membership is orthogonal to every other store
        // (the composer records a pad in the held store BESIDES this one, by its own explicit call)
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var recordedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var valueEqualCopy = new BaroquenNote(recordedNote);

        // act
        voiceRhythmLedger.RecordTexturePadNote(recordedNote);

        // assert
        voiceRhythmLedger.IsTexturePadNote(recordedNote).Should().BeTrue();
        voiceRhythmLedger.IsTexturePadNote(valueEqualCopy).Should().BeFalse("pad marks attach to the emitted instance, never to copies");
        voiceRhythmLedger.IsHeldNote(recordedNote).Should().BeFalse("the pad store never implies the held store; the composer records both explicitly");
        voiceRhythmLedger.IsTextureFigurationNote(recordedNote).Should().BeFalse("the pad store is orthogonal to the figuration store");
    }

    [Test]
    public void DivisionIntensities_AttachToTheInstance_NeverToValueEqualCopies()
    {
        // arrange - the same reference-identity contract as the role sets: a deep copy carries no intensity
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var recordedNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
        var valueEqualCopy = new BaroquenNote(recordedNote);

        // act
        voiceRhythmLedger.RecordDivisionIntensity(recordedNote, 60);

        // assert
        voiceRhythmLedger.TryGetDivisionIntensity(recordedNote, out var intensity).Should().BeTrue();
        intensity.Should().Be(60);
        voiceRhythmLedger.TryGetDivisionIntensity(valueEqualCopy, out _).Should().BeFalse("intensity attaches to the emitted instance, never to copies");
    }

    [Test]
    public void RecordingAnIntensityTwice_TheLastRecordingWins()
    {
        // arrange
        var voiceRhythmLedger = new VoiceRhythmLedger();
        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        // act
        voiceRhythmLedger.RecordDivisionIntensity(note, 60);
        voiceRhythmLedger.RecordDivisionIntensity(note, 140);

        // assert
        voiceRhythmLedger.TryGetDivisionIntensity(note, out var intensity).Should().BeTrue();
        intensity.Should().Be(140);
    }
}

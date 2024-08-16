using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Enums.Extensions;

[TestFixture]
internal sealed class NoteMotionExtensionsTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void FromNotes_returns_expected_NoteMotion(BaroquenNote note, BaroquenNote otherNote, NoteMotion expectedMotion)
    {
        NoteMotionExtensions.FromNotes(note, otherNote).Should().Be(expectedMotion);
        NoteMotionExtensions.FromNotes(note.Raw, otherNote.Raw).Should().Be(expectedMotion);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Half),
                NoteMotion.Descending
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                NoteMotion.Oblique
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half),
                NoteMotion.Ascending
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                NoteMotion.Ascending
            );

            yield return new TestCaseData(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Half),
                NoteMotion.Descending
            );
        }
    }
}

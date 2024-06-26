using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using FluentAssertions;
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
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.B3),
                NoteMotion.Descending
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.C4),
                NoteMotion.Oblique
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.CSharp4),
                NoteMotion.Ascending
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.C5),
                NoteMotion.Ascending
            );

            yield return new TestCaseData(
                new BaroquenNote(Voice.Soprano, Notes.C4),
                new BaroquenNote(Voice.Soprano, Notes.C3),
                NoteMotion.Descending
            );
        }
    }
}

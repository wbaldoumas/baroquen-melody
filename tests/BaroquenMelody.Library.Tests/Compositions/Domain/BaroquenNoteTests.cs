using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Domain;

[TestFixture]
internal sealed class BaroquenNoteTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Equality_methods_return_expected_results(BaroquenNote? note, BaroquenNote? otherNote, bool expectedEqualityResult)
    {
        // act + assert
        note?.Equals(otherNote).Should().Be(expectedEqualityResult);
        note?.Equals((object?)otherNote).Should().Be(expectedEqualityResult);
        (note == otherNote).Should().Be(expectedEqualityResult);
        (note != otherNote).Should().Be(!expectedEqualityResult);
    }

    [Test]
    public void GetHashCode_throws_InvalidOperationException()
    {
        // arrange
        var note = new BaroquenNote(Voice.Soprano, Notes.C1);

        // act
        var act = () => note.GetHashCode();

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var note = new BaroquenNote(Voice.Soprano, Notes.C1);
            var identicalNote = new BaroquenNote(Voice.Soprano, Notes.C1);
            var noteWithDifferentPitch = new BaroquenNote(Voice.Soprano, Notes.D1);
            var noteWithDifferentVoice = new BaroquenNote(Voice.Alto, Notes.C1);

            var noteWithDifferentDuration = new BaroquenNote(Voice.Soprano, Notes.C1)
            {
                Duration = note.Duration.Triplet()
            };

            var noteWithOrnamentation = new BaroquenNote(Voice.Soprano, Notes.C1)
            {
                Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.C2) }
            };

            BaroquenNote? nullNote = null;

            yield return new TestCaseData(note, note, true).SetName("Same note instances are equal");

            yield return new TestCaseData(note, identicalNote, true).SetName("Identical notes are equal");

            yield return new TestCaseData(note, noteWithDifferentPitch, false).SetName("Notes with different pitches are not equal");

            yield return new TestCaseData(note, noteWithDifferentVoice, false).SetName("Notes with different voices are not equal");

            yield return new TestCaseData(note, noteWithDifferentDuration, false).SetName("Notes with different durations are not equal");

            yield return new TestCaseData(note, noteWithOrnamentation, false).SetName("Notes with different ornamentations are not equal");

            yield return new TestCaseData(note, nullNote, false).SetName("Null note is not equal to any note");

            yield return new TestCaseData(nullNote, note, false).SetName("Null note is not equal to any note");
        }
    }
}

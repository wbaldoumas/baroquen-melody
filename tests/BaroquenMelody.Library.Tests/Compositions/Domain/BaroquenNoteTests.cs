using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Domain;

[TestFixture]
internal sealed class BaroquenNoteTests
{
    [Test]
    [TestCaseSource(nameof(EqualityTestCases))]
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
        var note = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half);

        // act
        var act = () => note.GetHashCode();

        // assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    [TestCaseSource(nameof(ComparisonTestCases))]
    public void Comparison_operators_should_correctly_compare_notes(BaroquenNote note, BaroquenNote otherNote, bool expectedIsGreaterThan)
    {
        // act + assert
        (note > otherNote).Should().Be(expectedIsGreaterThan);
        (note < otherNote).Should().Be(!expectedIsGreaterThan);
    }

    private static IEnumerable<TestCaseData> ComparisonTestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var sopranoC3 = new BaroquenNote(Instrument.One, Notes.C3, MusicalTimeSpan.Half);
            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);

            yield return new TestCaseData(sopranoC4, sopranoD4, false).SetName("C4 is less than D4");

            yield return new TestCaseData(sopranoD4, sopranoC4, true).SetName("D4 is greater than C4");

            yield return new TestCaseData(sopranoC4, sopranoC3, true).SetName("C4 is greater than C3");
        }
    }

    private static IEnumerable<TestCaseData> EqualityTestCases
    {
        get
        {
            var note = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half);
            var identicalNote = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half);
            var noteWithDifferentPitch = new BaroquenNote(Instrument.One, Notes.D1, MusicalTimeSpan.Half);
            var noteWithDifferentInstrument = new BaroquenNote(Instrument.Two, Notes.C1, MusicalTimeSpan.Half);

            var noteWithDifferentMusicalTimeSpan = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)
            {
                MusicalTimeSpan = note.MusicalTimeSpan.Triplet()
            };

            var noteWithOrnamentation = new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)
            {
                Ornamentations = { new BaroquenNote(Instrument.One, Notes.C2, MusicalTimeSpan.Half) }
            };

            BaroquenNote? nullNote = null;

            yield return new TestCaseData(note, note, true).SetName("Same note instances are equal");

            yield return new TestCaseData(note, identicalNote, true).SetName("Identical notes are equal");

            yield return new TestCaseData(note, noteWithDifferentPitch, false).SetName("Notes with different pitches are not equal");

            yield return new TestCaseData(note, noteWithDifferentInstrument, false).SetName("Notes with different instruments are not equal");

            yield return new TestCaseData(note, noteWithDifferentMusicalTimeSpan, false).SetName("Notes with different musical time spans are not equal");

            yield return new TestCaseData(note, noteWithOrnamentation, false).SetName("Notes with different ornamentations are not equal");

            yield return new TestCaseData(note, nullNote, false).SetName("Null note is not equal to any note");

            yield return new TestCaseData(nullNote, note, false).SetName("Null note is not equal to any note");
        }
    }
}

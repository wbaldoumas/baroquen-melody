using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class FugalAnswerStrategyTests
{
    // TestCompositionConfigurations.Get() is a C-Ionian scale: tonic C, dominant G.
    private readonly FugalAnswerStrategy _fugalAnswerStrategy = new(TestCompositionConfigurations.Get());

    [Test]
    [TestCaseSource(nameof(AnswerCases))]
    public void GenerateAnswer_ProducesTheExpectedRealOrTonalAnswer(IReadOnlyList<BaroquenNote> subject, Note[] expectedRawNotes)
    {
        var answer = _fugalAnswerStrategy.GenerateAnswer(subject);

        answer.Select(static note => note.Raw).Should().Equal(expectedRawNotes);
    }

    [Test]
    public void GenerateAnswer_transposes_ornamentations_with_their_parent_note()
    {
        // arrange: a tonic note decorated with a passing tone, opening off the tonic-dominant axis so it is a real answer.
        var subjectNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        subjectNote.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth));

        // act
        var answer = _fugalAnswerStrategy.GenerateAnswer([subjectNote]);

        // assert: E4 -> B4 (up a fifth), and its F4 passing tone -> C5 (also up a fifth, preserving the decoration).
        answer.Should().ContainSingle();
        answer[0].Raw.Should().Be(Notes.B4);
        answer[0].Ornamentations.Should().ContainSingle();
        answer[0].Ornamentations[0].Raw.Should().Be(Notes.C5);
    }

    private static IEnumerable<TestCaseData> AnswerCases()
    {
        yield return new TestCaseData(Subject(Notes.C4, Notes.D4, Notes.E4), new[] { Notes.G4, Notes.A4, Notes.B4 })
            .SetName("Real answer (up a fifth) when the head leaves the tonic-dominant axis");

        yield return new TestCaseData(Subject(Notes.C4, Notes.G4), new[] { Notes.G4, Notes.C5 })
            .SetName("Tonal answer flattens a dominant in the tonic-dominant head");

        yield return new TestCaseData(Subject(Notes.G4, Notes.C5), new[] { Notes.C5, Notes.G5 })
            .SetName("Tonal answer for a dominant-opening subject");

        yield return new TestCaseData(Subject(Notes.E4, Notes.F4), new[] { Notes.B4, Notes.C5 })
            .SetName("Real answer when the subject opens off the tonic-dominant axis");

        yield return new TestCaseData(Subject(Notes.G4, Notes.G4, Notes.A4), new[] { Notes.C5, Notes.C5, Notes.E5 })
            .SetName("Tonal answer flattens a repeated dominant head, then resumes the fifth");
    }

    private static List<BaroquenNote> Subject(params Note[] notes) =>
        notes.Select(static note => new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Half)).ToList();
}

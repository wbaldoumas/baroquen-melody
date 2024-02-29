using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System.Collections.Immutable;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations.Rules;

[TestFixture]
internal sealed class AvoidDissonanceTests
{
    private AvoidDissonance _avoidDissonance = null!;

    [SetUp]
    public void SetUp() => _avoidDissonance = new AvoidDissonance();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(ContextualizedChord currentChord, ContextualizedChord nextChord, bool expectedResult) =>
        _avoidDissonance.Evaluate(currentChord, nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            // the current chord is unnecessary for this test, so it is set to empty
            var unusedChord = new ContextualizedChord(ImmutableHashSet<ContextualizedNote>.Empty, ChordContext.Empty, ChordChoice.Empty);

            var sopranoC4 = new ContextualizedNote(
                Notes.C4,
                Voice.Soprano,
                new NoteContext(Voice.Soprano, Notes.C4, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            var altoE3 = new ContextualizedNote(
                Notes.E3,
                Voice.Alto,
                new NoteContext(Voice.Alto, Notes.E3, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            var tenorG2 = new ContextualizedNote(
                Notes.G2,
                Voice.Tenor,
                new NoteContext(Voice.Tenor, Notes.G2, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            var bassC1 = new ContextualizedNote(
                Notes.C1,
                Voice.Bass,
                new NoteContext(Voice.Bass, Notes.C1, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            var sopranoB4 = new ContextualizedNote(
                Notes.B4,
                Voice.Soprano,
                new NoteContext(Voice.Soprano, Notes.B4, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            var sopranoCSharp4 = new ContextualizedNote(
                Notes.CSharp4,
                Voice.Soprano,
                new NoteContext(Voice.Soprano, Notes.CSharp4, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            var bassASharp1 = new ContextualizedNote(
                Notes.ASharp1,
                Voice.Bass,
                new NoteContext(Voice.Bass, Notes.ASharp1, NoteMotion.Oblique, NoteSpan.None),
                NoteChoice.Empty
            );

            // consonant chords
            var cMajor = new ContextualizedChord(
                ImmutableHashSet.Create(sopranoC4, altoE3, tenorG2, bassC1),
                ChordContext.Empty,
                ChordChoice.Empty
            );

            var eMinor = new ContextualizedChord(
                ImmutableHashSet.Create(altoE3, tenorG2, sopranoB4),
                ChordContext.Empty,
                ChordChoice.Empty
            );

            // dissonant chords
            var cMajor7 = new ContextualizedChord(
                ImmutableHashSet.Create(sopranoB4, altoE3, tenorG2, bassC1),
                ChordContext.Empty,
                ChordChoice.Empty
            );

            var cSharpDiminished = new ContextualizedChord(
                ImmutableHashSet.Create(sopranoCSharp4, altoE3, tenorG2, bassASharp1),
                ChordContext.Empty,
                ChordChoice.Empty
            );

            var cSharpDiminishedMajor7 = new ContextualizedChord(
                ImmutableHashSet.Create(sopranoCSharp4, altoE3, tenorG2, bassC1),
                ChordContext.Empty,
                ChordChoice.Empty
            );

            yield return new TestCaseData(unusedChord, cMajor, true).SetName("Major triad is consonant.");

            yield return new TestCaseData(unusedChord, eMinor, true).SetName("Minor triad is consonant.");

            yield return new TestCaseData(unusedChord, cMajor7, false).SetName("Major 7th chord is dissonant.");

            yield return new TestCaseData(unusedChord, cSharpDiminished, false).SetName("Diminished chord is dissonant.");

            yield return new TestCaseData(unusedChord, cSharpDiminishedMajor7, false).SetName("Diminished major 7th chord is dissonant.");
        }
    }
}

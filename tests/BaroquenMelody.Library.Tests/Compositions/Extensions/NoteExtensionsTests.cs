using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Extensions;

[TestFixture]
internal sealed class NoteExtensionsTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void IsDissonantWith_ReturnsExpectedResult(Note note, Note otherNote, bool expectedResult) => note.IsDissonantWith(otherNote).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.C, 1), false)
                .SetName("Unison is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.CSharp, 1), true)
                .SetName("Half step is dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.D, 1), true)
                .SetName("Whole step is dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.DSharp, 1), false)
                .SetName("Minor third is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.E, 1), false)
                .SetName("Major third is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.F, 1), false)
                .SetName("Perfect fourth is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.FSharp, 1), true)
                .SetName("Tritone is dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.G, 1), false)
                .SetName("Perfect fifth is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.GSharp, 1), false)
                .SetName("Minor sixth is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.A, 1), false)
                .SetName("Major sixth is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.ASharp, 1), true)
                .SetName("Minor seventh is dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.B, 1), true)
                .SetName("Major seventh is dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.C, 2), false)
                .SetName("Octave is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.E, 5), false)
                .SetName("Wide range major third is not dissonant.");

            yield return new TestCaseData(Note.Get(NoteName.C, 1), Note.Get(NoteName.B, 6), true)
                .SetName("Wide range major seventh is dissonant.");
        }
    }
}

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
            yield return new TestCaseData(Notes.C1, Notes.C1, false)
                .SetName("Unison is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.CSharp1, true)
                .SetName("Half step is dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.D1, true)
                .SetName("Whole step is dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.DSharp1, false)
                .SetName("Minor third is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.E1, false)
                .SetName("Major third is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.F1, false)
                .SetName("Perfect fourth is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.FSharp1, true)
                .SetName("Tritone is dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.G1, false)
                .SetName("Perfect fifth is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.GSharp1, false)
                .SetName("Minor sixth is note dissonant");

            yield return new TestCaseData(Notes.C1, Notes.A1, false)
                .SetName("Major sixth is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.ASharp1, true)
                .SetName("Minor seventh is dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.B1, true)
                .SetName("Major seventh is dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.C2, false)
                .SetName("Octave is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.E5, false)
                .SetName("Wide range major third is not dissonant.");

            yield return new TestCaseData(Notes.C1, Notes.B6, true)
                .SetName("Wide range major seventh is dissonant.");
        }
    }
}

using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Chord = BaroquenMelody.Library.Compositions.Domain.Chord;
using Note = BaroquenMelody.Library.Compositions.Domain.Note;

namespace BaroquenMelody.Library.Tests.Compositions.Evaluations.Rules;

[TestFixture]
internal sealed class EnsureVoiceRangeTests
{
    private EnsureVoiceRange _ensureVoiceRange = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.C4),
                new(Voice.Alto, Notes.G2, Notes.G3),
                new(Voice.Tenor, Notes.C2, Notes.C3),
                new(Voice.Bass, Notes.G1, Notes.G2)
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _ensureVoiceRange = new EnsureVoiceRange(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(Chord nextChord, bool expectedResult) =>
        _ensureVoiceRange.Evaluate(default!, nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new Chord([new Note(Voice.Soprano, Notes.C3)]),
                true
            ).SetName("Soprano note is in range.");

            yield return new TestCaseData(
                new Chord([new Note(Voice.Soprano, Notes.B2)]),
                false
            ).SetName("Soprano note is out of range.");
        }
    }
}

using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class EnsureInstrumentRangeTests
{
    private EnsureInstrumentRange _ensureInstrumentRange = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _ensureInstrumentRange = new EnsureInstrumentRange(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(BaroquenChord nextChord, bool expectedResult) =>
        _ensureInstrumentRange.Evaluate(default!, nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half)]),
                true
            ).SetName("Soprano note is in range.");

            yield return new TestCaseData(
                new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B2, MusicalTimeSpan.Half)]),
                false
            ).SetName("Soprano note is out of range.");
        }
    }
}

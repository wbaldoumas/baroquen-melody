using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Domain;

[TestFixture]
internal sealed class BeatTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ContainsInstrument_returns_expected_result(Beat beat, Instrument instrument, bool expectedContainsInstrument)
    {
        // act
        var containsInstrument = beat.ContainsInstrument(instrument);

        // assert
        containsInstrument.Should().Be(expectedContainsInstrument);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)])),
                Instrument.One,
                true
            );

            yield return new TestCaseData(
                new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C1, MusicalTimeSpan.Half)])),
                Instrument.Two,
                false
            );
        }
    }
}

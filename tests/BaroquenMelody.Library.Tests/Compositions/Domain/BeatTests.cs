using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Domain;

[TestFixture]
internal sealed class BeatTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ContainsVoice_returns_expected_result(Beat beat, Voice voice, bool expectedContainsVoice)
    {
        // act
        var containsVoice = beat.ContainsVoice(voice);

        // assert
        containsVoice.Should().Be(expectedContainsVoice);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C1, MusicalTimeSpan.Half)])),
                Voice.Soprano,
                true
            );

            yield return new TestCaseData(
                new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C1, MusicalTimeSpan.Half)])),
                Voice.Alto,
                false
            );
        }
    }
}

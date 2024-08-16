using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Phrasing;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Phrasing;

[TestFixture]
internal sealed class ThemeSplitterTests
{
    private ThemeSplitter _themeSplitter = null!;

    [SetUp]
    public void SetUp() => _themeSplitter = new ThemeSplitter();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void SplitThemeIntoPhrases_GivenTheme_ReturnsPhrases(BaroquenTheme theme, List<RepeatedPhrase> expectedPhrases)
    {
        // act
        var actualPhrases = _themeSplitter.SplitThemeIntoPhrases(theme);

        // assert
        actualPhrases.Should().BeEquivalentTo(expectedPhrases);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var chordA = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]);
            var beatA = new Beat(chordA);
            var measureA = new Measure([beatA], Meter.FourFour);

            var chordB = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]);
            var beatB = new Beat(chordB);
            var measureB = new Measure([beatB], Meter.FourFour);

            var chordC = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]);
            var beatC = new Beat(chordC);
            var measureC = new Measure([beatC], Meter.FourFour);

            var chordD = new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)]);
            var beatD = new Beat(chordD);
            var measureD = new Measure([beatD], Meter.FourFour);

            var oneMeasurePhraseA = new List<Measure>
            {
                measureA
            };

            var oneMeasurePhraseB = new List<Measure>
            {
                measureB
            };

            var oneMeasurePhraseC = new List<Measure>
            {
                measureC
            };

            var oneMeasurePhraseD = new List<Measure>
            {
                measureD
            };

            var twoMeasurePhraseA = new List<Measure>
            {
                measureA, measureB
            };

            var twoMeasurePhraseB = new List<Measure>
            {
                measureB, measureC
            };

            var twoMeasurePhraseC = new List<Measure>
            {
                measureC, measureD
            };

            var threeMeasurePhraseA = new List<Measure>
            {
                measureA, measureB, measureC
            };

            var threeMeasurePhraseB = new List<Measure>
            {
                measureB, measureC, measureD
            };

            var fourMeasurePhrase = new List<Measure>
            {
                measureA, measureB, measureC, measureD
            };

            var themeWithOneMeasure = new BaroquenTheme
            {
                Recapitulation = [measureA]
            };

            var themeWithTwoMeasures = new BaroquenTheme
            {
                Recapitulation = [measureA, measureB]
            };

            var themeWithThreeMeasures = new BaroquenTheme
            {
                Recapitulation = [measureA, measureB, measureC]
            };

            var themeWithFourMeasures = new BaroquenTheme
            {
                Recapitulation = [measureA, measureB, measureC, measureD]
            };

            yield return new TestCaseData(themeWithOneMeasure, new List<RepeatedPhrase>
            {
                new() { Phrase = oneMeasurePhraseA }
            });

            yield return new TestCaseData(themeWithTwoMeasures, new List<RepeatedPhrase>
            {
                new() { Phrase = oneMeasurePhraseA },
                new() { Phrase = oneMeasurePhraseB },
                new() { Phrase = twoMeasurePhraseA }
            });

            yield return new TestCaseData(themeWithThreeMeasures, new List<RepeatedPhrase>
            {
                new() { Phrase = oneMeasurePhraseA },
                new() { Phrase = oneMeasurePhraseB },
                new() { Phrase = oneMeasurePhraseC },
                new() { Phrase = twoMeasurePhraseA },
                new() { Phrase = twoMeasurePhraseB },
                new() { Phrase = threeMeasurePhraseA }
            });

            yield return new TestCaseData(themeWithFourMeasures, new List<RepeatedPhrase>
            {
                new() { Phrase = oneMeasurePhraseA },
                new() { Phrase = oneMeasurePhraseB },
                new() { Phrase = oneMeasurePhraseC },
                new() { Phrase = oneMeasurePhraseD },
                new() { Phrase = twoMeasurePhraseA },
                new() { Phrase = twoMeasurePhraseB },
                new() { Phrase = twoMeasurePhraseC },
                new() { Phrase = threeMeasurePhraseA },
                new() { Phrase = threeMeasurePhraseB },
                new() { Phrase = fourMeasurePhrase }
            });
        }
    }
}

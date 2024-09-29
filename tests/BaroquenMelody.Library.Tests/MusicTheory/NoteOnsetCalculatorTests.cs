using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System.Collections;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class NoteOnsetCalculatorTests
{
    [Test]
    [TestCaseSource(nameof(CalculateNoteOnsetsTestCases))]
    public void CalculateNoteOnsets_returns_expected_value(BaroquenNote note, Meter meter, BitArray expectedBitArray)
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(2) with { Meter = meter };
        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();
        var noteOnsetCalculator = new NoteOnsetCalculator(musicalTimeSpanCalculator, compositionConfiguration);

        // act
        var result = noteOnsetCalculator.CalculateNoteOnsets(note.OrnamentationType);

        // assert
        for (var i = 0; i < expectedBitArray.Length; i++)
        {
            result[i].Should().Be(expectedBitArray[i]);
        }
    }

    private static IEnumerable<TestCaseData> CalculateNoteOnsetsTestCases()
    {
        // 4/4 time
        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        var expectedBitArray = new BitArray(16)
        {
            [0] = true
        };

        yield return new TestCaseData(note, Meter.FourFour, expectedBitArray).SetName($"Note without ornamentations returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
        {
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth)
            }
        };

        expectedBitArray = new BitArray(16)
        {
            [0] = true,
            [4] = true,
            [8] = true,
            [12] = true
        };

        yield return new TestCaseData(note, Meter.FourFour, expectedBitArray).SetName($"Note with run ornamentation returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter.Dotted(1))
        {
            OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth)
            }
        };

        expectedBitArray = new BitArray(16)
        {
            [0] = true,
            [12] = true,
            [14] = true
        };

        yield return new TestCaseData(note, Meter.FourFour, expectedBitArray).SetName($"Note with double run ornamentation returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth)
        {
            OrnamentationType = OrnamentationType.DelayedRepeatedNote,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth)
            }
        };

        expectedBitArray = new BitArray(16)
        {
            [0] = true,
            [12] = true
        };

        yield return new TestCaseData(note, Meter.FourFour, expectedBitArray).SetName($"Note with delayed repeated note ornamentation returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.Mordent,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter.Dotted(1))
            }
        };

        expectedBitArray = new BitArray(16)
        {
            [0] = true,
            [2] = true,
            [4] = true
        };

        yield return new TestCaseData(note, Meter.FourFour, expectedBitArray).SetName($"Note with mordent ornamentation returns {GetBitArrayString(expectedBitArray)}.");

        // 3/4 time
        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half.Dotted(1));

        expectedBitArray = new BitArray(24)
        {
            [0] = true
        };

        yield return new TestCaseData(note, Meter.ThreeFour, expectedBitArray).SetName($"Note without ornamentations returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter.Dotted(1))
        {
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth)
            }
        };

        expectedBitArray = new BitArray(24)
        {
            [0] = true,
            [12] = true,
            [16] = true,
            [20] = true
        };

        yield return new TestCaseData(note, Meter.ThreeFour, expectedBitArray).SetName($"Note with run ornamentation returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.DelayedDoublePassingTone,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Eighth)
            }
        };

        expectedBitArray = new BitArray(24)
        {
            [0] = true,
            [16] = true,
            [20] = true
        };

        yield return new TestCaseData(note, Meter.ThreeFour, expectedBitArray).SetName($"Note with double run ornamentation returns {GetBitArrayString(expectedBitArray)}.");

        note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.Mordent,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter.Dotted(1))
            }
        };

        expectedBitArray = new BitArray(24)
        {
            [0] = true,
            [2] = true,
            [4] = true
        };

        yield return new TestCaseData(note, Meter.ThreeFour, expectedBitArray).SetName($"Note with mordent ornamentation returns {GetBitArrayString(expectedBitArray)}.");
    }

    private static string GetBitArrayString(BitArray bitArray)
    {
        var tempResults = new List<string>();

        for (var i = 0; i < bitArray.Length; i++)
        {
            tempResults.Add(bitArray[i] ? "1" : "0");
        }

        return string.Join(", ", tempResults);
    }
}

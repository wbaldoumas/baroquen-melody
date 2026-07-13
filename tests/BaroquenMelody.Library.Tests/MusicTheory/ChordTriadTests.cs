using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class ChordTriadTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void FromChordNumber_ReturnsExpectedChordTones(BaroquenScale scale, ChordNumber chordNumber, ChordTriad? expectedChordTriad)
    {
        // act
        var chordTriad = ChordTriad.FromChordNumber(scale, chordNumber);

        // assert
        chordTriad.Should().Be(expectedChordTriad);
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        var cIonian = new BaroquenScale(NoteName.C, Mode.Ionian);

        yield return new TestCaseData(cIonian, ChordNumber.I, new ChordTriad(NoteName.C, NoteName.E, NoteName.G)).SetName("I in C Ionian is C-E-G.");
        yield return new TestCaseData(cIonian, ChordNumber.II, new ChordTriad(NoteName.D, NoteName.F, NoteName.A)).SetName("II in C Ionian is D-F-A.");
        yield return new TestCaseData(cIonian, ChordNumber.III, new ChordTriad(NoteName.E, NoteName.G, NoteName.B)).SetName("III in C Ionian is E-G-B.");
        yield return new TestCaseData(cIonian, ChordNumber.IV, new ChordTriad(NoteName.F, NoteName.A, NoteName.C)).SetName("IV in C Ionian is F-A-C.");
        yield return new TestCaseData(cIonian, ChordNumber.V, new ChordTriad(NoteName.G, NoteName.B, NoteName.D)).SetName("V in C Ionian is G-B-D.");
        yield return new TestCaseData(cIonian, ChordNumber.VI, new ChordTriad(NoteName.A, NoteName.C, NoteName.E)).SetName("VI in C Ionian is A-C-E.");
        yield return new TestCaseData(cIonian, ChordNumber.VII, new ChordTriad(NoteName.B, NoteName.D, NoteName.F)).SetName("VII in C Ionian is B-D-F.");
        yield return new TestCaseData(cIonian, ChordNumber.Unknown, null).SetName("An unknown chord number has no chord tones.");

        var aAeolian = new BaroquenScale(NoteName.A, Mode.Aeolian);

        yield return new TestCaseData(aAeolian, ChordNumber.V, new ChordTriad(NoteName.E, NoteName.G, NoteName.B)).SetName("V in A Aeolian is the diatonic minor v, E-G-B.");
        yield return new TestCaseData(aAeolian, ChordNumber.IV, new ChordTriad(NoteName.D, NoteName.F, NoteName.A)).SetName("IV in A Aeolian is D-F-A.");
    }
}

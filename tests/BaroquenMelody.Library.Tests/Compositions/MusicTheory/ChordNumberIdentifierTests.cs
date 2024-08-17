using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.MusicTheory;

[TestFixture]
internal sealed class ChordNumberIdentifierTests
{
    private ChordNumberIdentifier _chordNumberIdentifier = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

        _chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void IdentifyChordNumber_identifies_chord_number_as_expected(BaroquenChord chord, ChordNumber expectedChordNumber) =>
        _chordNumberIdentifier.IdentifyChordNumber(chord).Should().Be(expectedChordNumber);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half);

            var i = new BaroquenChord([sopranoC4, altoE3, tenorG2]);

            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);
            var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half);
            var tenorA2 = new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half);

            var ii = new BaroquenChord([sopranoD4, altoF3, tenorA2]);

            var sopranoE4 = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
            var altoG3 = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);
            var tenorB2 = new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half);

            var iii = new BaroquenChord([sopranoE4, altoG3, tenorB2]);

            var sopranoF4 = new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half);
            var altoA3 = new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half);
            var tenorC3 = new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half);

            var iv = new BaroquenChord([sopranoF4, altoA3, tenorC3]);

            var sopranoG4 = new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half);
            var altoB3 = new BaroquenNote(Instrument.Two, Notes.B3, MusicalTimeSpan.Half);
            var tenorD3 = new BaroquenNote(Instrument.Three, Notes.D3, MusicalTimeSpan.Half);

            var v = new BaroquenChord([sopranoG4, altoB3, tenorD3]);

            var sopranoA4 = new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half);
            var altoC4 = new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half);
            var tenorE3 = new BaroquenNote(Instrument.Three, Notes.E3, MusicalTimeSpan.Half);

            var vi = new BaroquenChord([sopranoA4, altoC4, tenorE3]);

            var sopranoB4 = new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half);
            var altoD4 = new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half);
            var tenorF3 = new BaroquenNote(Instrument.Three, Notes.F3, MusicalTimeSpan.Half);

            var vii = new BaroquenChord([sopranoB4, altoD4, tenorF3]);

            var unknown = new BaroquenChord([sopranoC4, altoE3, tenorF3]);

            yield return new TestCaseData(i, ChordNumber.I);
            yield return new TestCaseData(ii, ChordNumber.II);
            yield return new TestCaseData(iii, ChordNumber.III);
            yield return new TestCaseData(iv, ChordNumber.IV);
            yield return new TestCaseData(v, ChordNumber.V);
            yield return new TestCaseData(vi, ChordNumber.VI);
            yield return new TestCaseData(vii, ChordNumber.VII);
            yield return new TestCaseData(unknown, ChordNumber.Unknown);
        }
    }
}

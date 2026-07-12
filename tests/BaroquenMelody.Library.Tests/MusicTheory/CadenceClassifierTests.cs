using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.MusicTheory;

[TestFixture]
internal sealed class CadenceClassifierTests
{
    [Test]
    [TestCaseSource(nameof(CIonianTestCases))]
    public void ClassifyCadence_InCIonian_ReturnsExpectedCadenceType(BaroquenChord penultimateChord, BaroquenChord finalChord, CadenceType expectedCadenceType)
    {
        // arrange
        var cadenceClassifier = CreateCadenceClassifier(TestCompositionConfigurations.Get());

        // act
        var cadenceType = cadenceClassifier.ClassifyCadence(penultimateChord, finalChord);

        // assert
        cadenceType.Should().Be(expectedCadenceType);
    }

    [Test]
    public void ClassifyCadence_WhenFirstInversionIvDescendsBySemitoneToV_ReturnsPhrygianHalf()
    {
        // arrange - in A Aeolian the iv6 bass (F) sits a semitone above the dominant (E)
        var cadenceClassifier = CreateCadenceClassifier(TestCompositionConfigurations.Get(tonic: NoteName.A, mode: Mode.Aeolian));

        var penultimateChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.D2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.F1, MusicalTimeSpan.Half)
        ]);

        var finalChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.E1, MusicalTimeSpan.Half)
        ]);

        // act
        var cadenceType = cadenceClassifier.ClassifyCadence(penultimateChord, finalChord);

        // assert
        cadenceType.Should().Be(CadenceType.PhrygianHalf);
    }

    [Test]
    public void ClassifyCadence_WhenFirstInversionIvBassLeapsToV_ReturnsHalf()
    {
        // arrange - the iv6 bass lands on the dominant from an octave-plus-semitone above, not a semitone step
        var cadenceClassifier = CreateCadenceClassifier(TestCompositionConfigurations.Get(tonic: NoteName.A, mode: Mode.Aeolian));

        var penultimateChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.D2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.F2, MusicalTimeSpan.Half)
        ]);

        var finalChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.E1, MusicalTimeSpan.Half)
        ]);

        // act
        var cadenceType = cadenceClassifier.ClassifyCadence(penultimateChord, finalChord);

        // assert
        cadenceType.Should().Be(CadenceType.Half);
    }

    [Test]
    public void ClassifyCadence_WhenFinalChordVoicesNoConfiguredInstrument_ReturnsHalf()
    {
        // arrange - a two-instrument configuration whose final chord is voiced only by unconfigured
        // instruments, so no bass note is available for the Phrygian semitone check. The penultimate {A, F}
        // is exclusively a first-inversion iv (unlike {D, F}, which the identifier would resolve to II first),
        // keeping the Phrygian condition alive up to the missing-bass check.
        var cadenceClassifier = CreateCadenceClassifier(TestCompositionConfigurations.Get(2, tonic: NoteName.A, mode: Mode.Aeolian));

        var penultimateChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half)
        ]);

        var finalChord = new BaroquenChord([
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.B1, MusicalTimeSpan.Half)
        ]);

        // act
        var cadenceType = cadenceClassifier.ClassifyCadence(penultimateChord, finalChord);

        // assert
        cadenceType.Should().Be(CadenceType.Half);
    }

    private static CadenceClassifier CreateCadenceClassifier(CompositionConfiguration compositionConfiguration)
    {
        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

        return new CadenceClassifier(
            chordNumberIdentifier,
            new ChordInversionIdentifier(chordNumberIdentifier, compositionConfiguration),
            compositionConfiguration);
    }

    private static IEnumerable<TestCaseData> CIonianTestCases()
    {
        var rootPositionV = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
        ]);

        var rootPositionIWithTonicSoprano = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(rootPositionV, rootPositionIWithTonicSoprano, CadenceType.PerfectAuthentic)
            .SetName("Root position V to root position I with the tonic in the soprano is a perfect authentic cadence.");

        var firstInversionV = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.B1, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(firstInversionV, rootPositionIWithTonicSoprano, CadenceType.ImperfectAuthentic)
            .SetName("An inverted V resolving to I is an imperfect authentic cadence.");

        var rootPositionIWithMediantSoprano = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(rootPositionV, rootPositionIWithMediantSoprano, CadenceType.ImperfectAuthentic)
            .SetName("V to I with the soprano off the tonic is an imperfect authentic cadence.");

        var firstInversionI = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.E1, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(rootPositionV, firstInversionI, CadenceType.ImperfectAuthentic)
            .SetName("V to an inverted I is an imperfect authentic cadence.");

        var rootPositionI = rootPositionIWithTonicSoprano;

        yield return new TestCaseData(rootPositionI, rootPositionV, CadenceType.Half)
            .SetName("I moving to V is a half cadence.");

        var rootPositionII = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.D2, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(rootPositionII, rootPositionV, CadenceType.Half)
            .SetName("II moving to V is a half cadence.");

        var firstInversionIV = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.F2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.A1, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(firstInversionIV, rootPositionV, CadenceType.Half)
            .SetName("In Ionian the IV6 bass falls a whole step to V, making a plain half cadence.");

        var rootPositionVI = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.A1, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(rootPositionV, rootPositionVI, CadenceType.Deceptive)
            .SetName("V resolving to VI is a deceptive cadence.");

        var rootPositionIV = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.F1, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(rootPositionIV, rootPositionIWithTonicSoprano, CadenceType.Plagal)
            .SetName("IV resolving to I is a plagal cadence.");

        yield return new TestCaseData(rootPositionIV, rootPositionV, CadenceType.Half)
            .SetName("A root position IV moving to V is a plain half cadence.");

        yield return new TestCaseData(rootPositionI, rootPositionI, CadenceType.None)
            .SetName("A repeated chord number forms no cadence.");

        var chromaticChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.FSharp4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.D2, MusicalTimeSpan.Half)
        ]);

        yield return new TestCaseData(chromaticChord, rootPositionIWithTonicSoprano, CadenceType.None)
            .SetName("An unidentifiable penultimate chord forms no cadence.");

        yield return new TestCaseData(rootPositionV, chromaticChord, CadenceType.None)
            .SetName("An unidentifiable final chord forms no cadence.");

        yield return new TestCaseData(rootPositionV, rootPositionIV, CadenceType.None)
            .SetName("V falling back to IV forms no cadence.");
    }
}

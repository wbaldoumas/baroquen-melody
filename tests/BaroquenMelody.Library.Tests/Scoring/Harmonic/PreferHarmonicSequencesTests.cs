using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Scoring.Harmonic;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Scoring.Harmonic;

[TestFixture]
internal sealed class PreferHarmonicSequencesTests
{
    private PreferHarmonicSequences _preferHarmonicSequences = null!;

    [SetUp]
    public void SetUp() => _preferHarmonicSequences = new PreferHarmonicSequences(
        new ChordNumberIdentifier(TestCompositionConfigurations.Get()));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Score_ReturnsExpectedPenalty(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, double expectedPenalty)
    {
        // act
        var penalty = _preferHarmonicSequences.Score(precedingChords, nextChord);

        // assert
        penalty.Should().Be(expectedPenalty);
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        // Root position triads in C Ionian; descending-fifth root motion is (degree + 3) mod 7.
        var chordI = CreateChord(Notes.C2, Notes.G2, Notes.E3, Notes.C4);
        var chordII = CreateChord(Notes.D2, Notes.A2, Notes.F3, Notes.D4);
        var chordIII = CreateChord(Notes.E2, Notes.B2, Notes.G3, Notes.E4);
        var chordIV = CreateChord(Notes.F2, Notes.C3, Notes.A3, Notes.F4);
        var chordVII = CreateChord(Notes.B1, Notes.D3, Notes.F3, Notes.B4);
        var chordV = CreateChord(Notes.G1, Notes.B2, Notes.D3, Notes.G4);
        var chordVI = CreateChord(Notes.A1, Notes.C3, Notes.E3, Notes.A4);
        var chromaticChord = CreateChord(Notes.D2, Notes.A2, Notes.D3, Notes.FSharp4);

        yield return new TestCaseData(new List<BaroquenChord> { chordVI }, chordII, 0d)
            .SetName("A single preceding chord establishes no motion and is neutral.");

        yield return new TestCaseData(new List<BaroquenChord> { chromaticChord, chordII }, chordV, 0d)
            .SetName("An unidentifiable earlier chord leaves the rule neutral.");

        yield return new TestCaseData(new List<BaroquenChord> { chordVI, chromaticChord }, chordV, 0d)
            .SetName("An unidentifiable last chord leaves the rule neutral.");

        yield return new TestCaseData(new List<BaroquenChord> { chordVI, chordII }, chromaticChord, 0d)
            .SetName("An unidentifiable candidate is neutral.");

        yield return new TestCaseData(new List<BaroquenChord> { chordII, chordII }, chordV, 0d)
            .SetName("A repeated chord number establishes no motion and is neutral.");

        yield return new TestCaseData(new List<BaroquenChord> { chordVI, chordII }, chordV, 0d)
            .SetName("Continuing a freshly established descending-fifth motion costs nothing.");

        yield return new TestCaseData(new List<BaroquenChord> { chordVI, chordII }, chordIII, 1d)
            .SetName("Breaking a freshly established motion costs one.");

        yield return new TestCaseData(new List<BaroquenChord> { chordIII, chordVI, chordII }, chordV, 1d)
            .SetName("Extending a motion that already repeated twice costs one, releasing the sequence.");

        yield return new TestCaseData(new List<BaroquenChord> { chordIII, chordVI, chordII }, chordI, 0d)
            .SetName("Breaking a saturated sequence costs nothing.");

        yield return new TestCaseData(new List<BaroquenChord> { chordI, chordVI, chordII }, chordV, 0d)
            .SetName("A different earlier motion leaves the sequence unsaturated, so continuing costs nothing.");

        yield return new TestCaseData(new List<BaroquenChord> { chromaticChord, chordVI, chordII }, chordV, 0d)
            .SetName("An unidentifiable chord before the established motion cannot saturate the sequence.");

        // Audit: search-scoring-random-1 - a held-harmony duplicate must not erase the established motion.
        yield return new TestCaseData(new List<BaroquenChord> { chordI, chordIV, new BaroquenChord(chordIV) }, chordVII, 0d)
            .SetName("Continuing a motion established across a held duplicate costs nothing.");

        yield return new TestCaseData(new List<BaroquenChord> { chordI, chordIV, new BaroquenChord(chordIV) }, chordV, 1d)
            .SetName("Breaking a motion established across a held duplicate costs one.");
    }

    private static BaroquenChord CreateChord(Note four, Note three, Note two, Note one) => new([
        new BaroquenNote(Instrument.One, one, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, two, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Three, three, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Four, four, MusicalTimeSpan.Half)
    ]);
}

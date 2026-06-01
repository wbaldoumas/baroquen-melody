using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules;

[TestFixture]
internal sealed class EnforceVoiceSpacingTests
{
    private EnforceVoiceSpacing _enforceVoiceSpacing = null!;

    [SetUp]
    public void SetUp() => _enforceVoiceSpacing = new EnforceVoiceSpacing(TestCompositionConfigurations.Get(4));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult)
    {
        var result = _enforceVoiceSpacing.Evaluate(precedingChords, nextChord);

        result.Should().Be(expectedResult);
    }

    // register order (high -> low) for Get(4) is One, Two, Three, Four.
    // restricted (upper) adjacent pairs: One-Two and Two-Three. The lowest pair (Three-Four) is unrestricted.
    private static IEnumerable<TestCaseData> TestCases()
    {
        var noPrecedingChords = new List<BaroquenChord>();

        // upper pairs within an octave (soprano-alto exactly an octave) -> allowed.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Upper pairs within an octave are allowed (octave boundary inclusive)");

        // soprano-alto wider than an octave -> rejected.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Soprano-alto wider than an octave is rejected");

        // alto-tenor wider than an octave -> rejected.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Alto-tenor wider than an octave is rejected");

        // the lowest (bass-tenor) pair may span more than an octave.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Bass-tenor may span more than an octave");

        // two voices: the only pair is the lowest pair, so any spacing is allowed.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Two voices may span more than an octave");

        // three voices: soprano-alto wider than an octave -> rejected.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C2, MusicalTimeSpan.Half)
            ]),
            false
        ).SetName("Three voices reject a wide soprano-alto spacing");

        // three voices: the lowest pair (alto-tenor here) may span more than an octave.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.C2, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Three voices allow a wide lowest pair");

        // a single voice has no pairs to space.
        yield return new TestCaseData(
            noPrecedingChords,
            new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
            ]),
            true
        ).SetName("Single voice has no spacing constraint");
    }
}

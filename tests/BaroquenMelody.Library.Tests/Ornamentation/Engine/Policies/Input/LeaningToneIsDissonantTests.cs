using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class LeaningToneIsDissonantTests
{
    private LeaningToneIsDissonant _leaningToneIsDissonant = null!;

    [SetUp]
    public void SetUp() => _leaningToneIsDissonant = new LeaningToneIsDissonant(TestCompositionConfigurations.Get(2));

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess_ShouldReturnExpectedResult(OrnamentationItem ornamentationItem, InputPolicyResult expectedResult)
    {
        // act
        var result = _leaningToneIsDissonant.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            // the leaning tone above C4 is D: no voice holds a D, so the dissonance is genuine
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName("When no voice holds the leaning tone, policy continues.");

            // the leaning tone above C4 is D: another voice holds D3, so the leaning tone is a chord tone
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When another voice holds the leaning tone's pitch class in any octave, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When another voice holds the leaning tone in unison octave, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Two,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When the instrument is not present in the beat, policy rejects.");

            // C#4 is not in the C major scale, so the leaning tone cannot be classified
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.CSharp4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When the target note is not a scale note, policy rejects.");
        }
    }
}

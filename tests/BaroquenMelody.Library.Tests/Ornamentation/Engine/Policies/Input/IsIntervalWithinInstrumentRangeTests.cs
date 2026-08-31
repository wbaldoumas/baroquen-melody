using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsIntervalWithinInstrumentRangeTests
{
    private IsIntervalWithinInstrumentRange _isIntervalWithinInstrumentRange;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get();

        _isIntervalWithinInstrumentRange = new IsIntervalWithinInstrumentRange(compositionConfiguration, interval: 5);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem ornamentationItem, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var result = _isIntervalWithinInstrumentRange.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedInputPolicyResult);
    }

    // In B Aeolian the scale's note list starts at B(-1)/C0, so A0 sits at index 6; the octave-pedal guard's -7 offset
    // used to index the list at -1 and throw instead of rejecting the out-of-range interval.
    [Test]
    public void ShouldProcess_rejects_instead_of_throwing_when_the_interval_falls_below_the_scale_list()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(tonic: NoteName.B, mode: Mode.Aeolian);
        var policy = new IsIntervalWithinInstrumentRange(compositionConfiguration, interval: -7);

        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A0, MusicalTimeSpan.Half)])),
            null
        );

        // act
        var act = () => policy.ShouldProcess(ornamentationItem);

        // assert
        act.Should().NotThrow("an interval that leaves the scale's note list is simply not within the instrument range");
        act().Should().Be(InputPolicyResult.Reject);
    }

    // A note outside the scale cannot anchor a scale-relative interval, so the site must be rejected outright:
    // C natural is not in B Aeolian, and falling through with IndexOf's -1 would range-check notes[8] = C#1 -
    // a note inside Instrument.Four's C1-C2 range - and wrongly accept the site. Unreachable during composition
    // (every composed note is drawn from the scale), but the guard's verdict must stay correct for any input.
    [Test]
    public void ShouldProcess_rejects_when_the_current_note_is_not_in_the_scale()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(tonic: NoteName.B, mode: Mode.Aeolian);
        var policy = new IsIntervalWithinInstrumentRange(compositionConfiguration, interval: 9);

        var ornamentationItem = new OrnamentationItem(
            Instrument.Four,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)])),
            null
        );

        // act
        var result = policy.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(InputPolicyResult.Reject);
    }

    // The symmetric edge: in C Ionian only five scale notes sit above B8 (C9..G9), so an upper-octave +7 offset from
    // B8 leaves the top of the note list.
    [Test]
    public void ShouldProcess_rejects_instead_of_throwing_when_the_interval_rises_above_the_scale_list()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var policy = new IsIntervalWithinInstrumentRange(compositionConfiguration, interval: 7);

        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B8, MusicalTimeSpan.Half)])),
            null
        );

        // act
        var act = () => policy.ShouldProcess(ornamentationItem);

        // assert
        act.Should().NotThrow("an interval that leaves the scale's note list is simply not within the instrument range");
        act().Should().Be(InputPolicyResult.Reject);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var testCompositionContext = new FixedSizeList<Beat>(1);

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName($"When added interval is within instrument range, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName($"When added interval is not within instrument range, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

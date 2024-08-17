using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsApplicableIntervalTests
{
    private IsApplicableInterval _isApplicableInterval = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _isApplicableInterval = new IsApplicableInterval(compositionConfiguration, Interval: 2);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess(OrnamentationItem item, InputPolicyResult expectedInputPolicyResult)
    {
        // act
        var inputPolicyResult = _isApplicableInterval.ShouldProcess(item);

        // assert
        inputPolicyResult.Should().Be(expectedInputPolicyResult);
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
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Continue
            ).SetName($"When notes are a third apart, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName($"When notes are not a third apart, then {nameof(InputPolicyResult.Reject)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName($"When the next note is null, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

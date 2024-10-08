﻿using Atrea.PolicyEngine.Policies.Input;
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
internal sealed class IsNextNoteIntervalWithinInstrumentRangeTests
{
    private IsNextNoteIntervalWithinInstrumentRange _isIntervalWithinInstrumentRange;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get();

        _isIntervalWithinInstrumentRange = new IsNextNoteIntervalWithinInstrumentRange(compositionConfiguration, interval: 5);
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
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Continue
            ).SetName($"When added interval is within instrument range, then {nameof(InputPolicyResult.Continue)} is returned.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    testCompositionContext,
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName($"When added interval is not within instrument range, then {nameof(InputPolicyResult.Reject)} is returned.");
        }
    }
}

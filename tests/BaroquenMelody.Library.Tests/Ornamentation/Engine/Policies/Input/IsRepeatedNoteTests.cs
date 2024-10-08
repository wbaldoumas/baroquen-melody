﻿using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsRepeatedNoteTests
{
    private IsRepeatedNote _isRepeatedNote = null!;

    [SetUp]
    public void SetUp() => _isRepeatedNote = new IsRepeatedNote();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess_ShouldReturnExpectedResult(OrnamentationItem ornamentationItem, InputPolicyResult expectedResult)
    {
        // act
        var result = _isRepeatedNote.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Continue
            ).SetName("When notes are repeated, policy continues.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName("When notes are not repeated, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]))
                ),
                InputPolicyResult.Reject
            ).SetName("When instrument is not present, policy rejects.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
                    null
                ),
                InputPolicyResult.Reject
            ).SetName("When next beat is null, policy rejects.");
        }
    }
}

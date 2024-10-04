using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Policies.Input;
using BaroquenMelody.Library.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics.Engine.Policies;

[TestFixture]
internal sealed class HasProcessedCurrentBeatTests
{
    private HasProcessedCurrentBeat _hasProcessedCurrentBeat = null!;

    [SetUp]
    public void SetUp() => _hasProcessedCurrentBeat = new HasProcessedCurrentBeat();

    [Test]
    [TestCase(true, InputPolicyResult.Continue)]
    [TestCase(false, InputPolicyResult.Reject)]
    public void ShouldProcess_returns_expected_result(bool hasProcessedCurrentBeat, InputPolicyResult expectedResult)
    {
        // arrange
        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            NextBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            ProcessedInstruments = [],
            HasProcessedCurrentBeat = hasProcessedCurrentBeat
        };

        // act
        var result = _hasProcessedCurrentBeat.ShouldProcess(item);

        // assert
        result.Should().Be(expectedResult);
    }
}

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
internal sealed class InstrumentIsPresentInCurrentBeatTests
{
    private InstrumentIsPresentInCurrentBeat _instrumentIsPresentInCurrentBeat = null!;

    [SetUp]
    public void SetUp() => _instrumentIsPresentInCurrentBeat = new InstrumentIsPresentInCurrentBeat();

    [Test]
    [TestCase(Instrument.One, InputPolicyResult.Continue)]
    [TestCase(Instrument.Two, InputPolicyResult.Reject)]
    public void ShouldProcess_returns_expected_result(Instrument instrument, InputPolicyResult expectedInputPolicyResult)
    {
        // arrange
        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            NextBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            ProcessedInstruments = []
        };

        // act
        var result = _instrumentIsPresentInCurrentBeat.ShouldProcess(item);

        // assert
        result.Should().Be(InputPolicyResult.Continue);
    }
}

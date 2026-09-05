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
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class HasNeighborNotesWithinInstrumentRangeTests
{
    private HasNeighborNotesWithinInstrumentRange _policy = null!;

    [SetUp]
    public void SetUp() => _policy = new HasNeighborNotesWithinInstrumentRange(TestCompositionConfigurations.Get());

    [Test]
    public void ShouldProcess_WhenBothScaleNeighborsAreInRange_Continues()
    {
        // arrange - Instrument.One spans C4..C6, so E4's neighbours D4 and F4 both fit
        var item = CreateItem(Notes.E4);

        // act
        var result = _policy.ShouldProcess(item);

        // assert
        result.Should().Be(InputPolicyResult.Continue);
    }

    [Test]
    public void ShouldProcess_WhenTheUpperNeighborIsOutOfRange_Rejects()
    {
        // arrange - C6 is the top of the range; its upper neighbour D6 is not playable
        var item = CreateItem(Notes.C6);

        // act
        var result = _policy.ShouldProcess(item);

        // assert
        result.Should().Be(InputPolicyResult.Reject);
    }

    [Test]
    public void ShouldProcess_WhenTheLowerNeighborIsOutOfRange_Rejects()
    {
        // arrange - C4 is the bottom of the range; its lower neighbour B3 is not playable
        var item = CreateItem(Notes.C4);

        // act
        var result = _policy.ShouldProcess(item);

        // assert
        result.Should().Be(InputPolicyResult.Reject);
    }

    private static OrnamentationItem CreateItem(Note note) => new(
        Instrument.One,
        new FixedSizeList<Beat>(1),
        new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Half)])),
        null
    );
}

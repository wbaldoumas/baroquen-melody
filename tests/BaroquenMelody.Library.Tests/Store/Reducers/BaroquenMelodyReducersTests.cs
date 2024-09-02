using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class BaroquenMelodyReducersTests
{
    [Test]
    public void ReduceUpdateBaroquenMelody()
    {
        // arrange
        var state = new BaroquenMelodyState();
        var action = new UpdateBaroquenMelody(new BaroquenMelody(new MidiFile()));

        // act
        var newState = BaroquenMelodyReducers.ReduceUpdateBaroquenMelody(state, action);

        // assert
        newState.BaroquenMelody.Should().Be(action.BaroquenMelody);
    }
}

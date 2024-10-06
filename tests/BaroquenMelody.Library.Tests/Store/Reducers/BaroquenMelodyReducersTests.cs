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
        var action = new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile()), "Test", true);

        // act
        var newState = BaroquenMelodyReducers.ReduceUpdateBaroquenMelody(state, action);

        // assert
        newState.Composition.Should().Be(action.MidiFileComposition);
        newState.Path.Should().Be(action.Path);
        newState.HasBeenSaved.Should().BeTrue();
    }

    [Test]
    public void ReduceMarkCompositionSaved()
    {
        // arrange
        var state = new BaroquenMelodyState { HasBeenSaved = false };
        var action = new MarkCompositionSaved();

        // act
        var newState = BaroquenMelodyReducers.ReduceMarkCompositionSaved(state, action);

        // assert
        newState.HasBeenSaved.Should().BeTrue();
    }
}

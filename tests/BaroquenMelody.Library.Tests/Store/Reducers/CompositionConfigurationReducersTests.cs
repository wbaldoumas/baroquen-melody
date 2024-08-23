using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class CompositionConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateCompositionConfiguration_updates_composition_configurations_as_expected()
    {
        // arrange
        const NoteName rootNote = NoteName.C;
        const Mode mode = Mode.Ionian;
        var state = new CompositionConfigurationState();

        // act
        state = CompositionConfigurationReducers.ReduceUpdateCompositionConfiguration(state, new UpdateCompositionConfiguration(rootNote, mode, Meter.ThreeFour, 8));

        // assert
        state.Meter.Should().Be(Meter.ThreeFour);
        state.MinimumMeasures.Should().Be(8);
        state.TonicNote.Should().Be(rootNote);
        state.Mode.Should().Be(mode);
    }
}

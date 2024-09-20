using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using BaroquenMelody.Library.Tests.TestData;
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
        state = CompositionConfigurationReducers.ReduceUpdateCompositionConfiguration(state, new UpdateCompositionConfiguration(rootNote, mode, Meter.ThreeFour, 8, 555));

        // assert
        state.Meter.Should().Be(Meter.ThreeFour);
        state.MinimumMeasures.Should().Be(8);
        state.TonicNote.Should().Be(rootNote);
        state.Mode.Should().Be(mode);
        state.Tempo.Should().Be(555);
    }

    [Test]
    public void ReduceLoadCompositionConfiguration_updates_composition_configurations_as_expected()
    {
        // arrange
        var configuration = Configurations.GetCompositionConfiguration();
        var state = new CompositionConfigurationState();

        // act
        state = CompositionConfigurationReducers.ReduceLoadCompositionConfiguration(state, new LoadCompositionConfiguration(configuration));

        // assert
        state.Meter.Should().Be(configuration.Meter);
        state.MinimumMeasures.Should().Be(configuration.MinimumMeasures);
        state.TonicNote.Should().Be(configuration.Tonic);
        state.Mode.Should().Be(configuration.Mode);
        state.Tempo.Should().Be(configuration.Tempo);
    }
}

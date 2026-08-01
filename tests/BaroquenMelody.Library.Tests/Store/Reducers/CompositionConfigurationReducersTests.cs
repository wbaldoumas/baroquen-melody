using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
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
        state = CompositionConfigurationReducers.ReduceUpdateCompositionConfiguration(state, new UpdateCompositionConfiguration(rootNote, mode, Meter.ThreeFour, 8, 555, CompositionForm.GroundBass, GroundBass.Romanesca, GroundBassModulate: false));

        // assert
        state.Meter.Should().Be(Meter.ThreeFour);
        state.MinimumMeasures.Should().Be(8);
        state.TonicNote.Should().Be(rootNote);
        state.Mode.Should().Be(mode);
        state.Tempo.Should().Be(555);
        state.Form.Should().Be(CompositionForm.GroundBass);
        state.GroundBassPattern.Should().Be(GroundBass.Romanesca);
        state.GroundBassModulate.Should().BeFalse("the action's toggle must land in the store");
    }

    [Test]
    public void ReduceLoadCompositionConfiguration_updates_composition_configurations_as_expected()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get();
        var state = new CompositionConfigurationState();

        // act
        state = CompositionConfigurationReducers.ReduceLoadCompositionConfiguration(state, new LoadCompositionConfiguration(configuration));

        // assert
        state.Meter.Should().Be(configuration.Meter);
        state.MinimumMeasures.Should().Be(configuration.MinimumMeasures);
        state.TonicNote.Should().Be(configuration.Tonic);
        state.Mode.Should().Be(configuration.Mode);
        state.Tempo.Should().Be(configuration.Tempo);
        state.Form.Should().Be(CompositionForm.Fugue, "a configuration without a ground bass section is the standard form");
        state.GroundBassPattern.Should().BeNull("a configuration without a ground bass section pins no pattern");
        state.GroundBassModulate.Should().BeTrue("a configuration without a ground bass section keeps the default journey");
    }

    [Test]
    public void ReduceLoadCompositionConfiguration_maps_an_enabled_ground_bass_configuration_to_the_ground_bass_form()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get() with { GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.CadentialGround, Modulate: false) };
        var state = new CompositionConfigurationState();

        // act
        state = CompositionConfigurationReducers.ReduceLoadCompositionConfiguration(state, new LoadCompositionConfiguration(configuration));

        // assert
        state.Form.Should().Be(CompositionForm.GroundBass);
        state.GroundBassPattern.Should().Be(GroundBass.CadentialGround, "the loaded configuration's pinned pattern must surface in the store");
        state.GroundBassModulate.Should().BeFalse("the loaded configuration's modulation toggle must surface in the store");
    }

    [Test]
    public void ReduceLoadCompositionConfiguration_maps_a_disabled_ground_bass_configuration_to_the_fugue_form()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get() with { GroundBassConfiguration = new GroundBassConfiguration(Enabled: false) };
        var state = new CompositionConfigurationState { Form = CompositionForm.GroundBass };

        // act
        state = CompositionConfigurationReducers.ReduceLoadCompositionConfiguration(state, new LoadCompositionConfiguration(configuration));

        // assert
        state.Form.Should().Be(CompositionForm.Fugue);
    }
}

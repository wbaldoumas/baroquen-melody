using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.App.Components.Tests.TestData;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionConfigurationPanelTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Panel_renders_the_composition_configuration_card()
    {
        // act
        var component = _testContext.RenderComponent<CompositionConfigurationPanel>();

        // assert
        component.FindComponents<CompositionConfigurationCard>().Should().ContainSingle();
    }

    [Test]
    public void Randomize_with_the_ground_bass_form_rolls_a_pattern_that_fits_through_the_real_service()
    {
        // arrange: a G3-B4 ground-hosting voice means feasibility varies by key, so the button's roll
        // must consult the same analyzer the composer will - this drives the real service through the
        // real container, not a mock, and ten clicks sample enough keys to make the property meaningful.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        var component = _testContext.RenderComponent<CompositionConfigurationPanel>();
        var groundBassFeasibilityAnalyzer = _testContext.Services.GetRequiredService<IGroundBassFeasibilityAnalyzer>();

        for (var roll = 0; roll < 10; ++roll)
        {
            // act
            component.ClickButtonByText("Randomize");

            // assert
            var state = _testContext.StateOf<CompositionConfigurationState>();

            state.Form.Should().Be(CompositionForm.GroundBass, "randomizing keeps the selected form");

            if (state.GroundBassPattern is { } pattern)
            {
                groundBassFeasibilityAnalyzer.GetFeasibleGroundBasses(
                    _testContext.StateOf<InstrumentConfigurationState>().EnabledConfigurations,
                    state.Scale
                ).Should().Contain(pattern, "the rolled pattern must fit the rolled key {0} {1}", state.TonicNote, state.Mode);
            }
        }
    }

    [Test]
    public void Randomize_with_the_ground_bass_form_rolls_the_pattern_against_the_ranges_the_rolled_key_snaps_to()
    {
        // arrange: changing the key re-snaps every voice's last user-applied range to the new scale, so by
        // the time the rolled pattern lands in state, the ground-hosting voice may sit on different bounds
        // than the ones the roll consulted. Recreating that drift before every click - user-applied G3-B4,
        // current range widened to B1-B5 as a previous key's snap would leave it - makes a roll that
        // consults the stale current ranges pick infeasible patterns almost surely within a few clicks.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        var component = _testContext.RenderComponent<CompositionConfigurationPanel>();
        var groundBassFeasibilityAnalyzer = _testContext.Services.GetRequiredService<IGroundBassFeasibilityAnalyzer>();

        for (var roll = 0; roll < 25; ++roll)
        {
            var groundHostingVoice = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.Three]!;

            _testContext.Dispatcher.Dispatch(
                new UpdateInstrumentConfiguration(
                    Instrument.Three,
                    Notes.B1,
                    Notes.B5,
                    groundHostingVoice.MinVelocity,
                    groundHostingVoice.MaxVelocity,
                    groundHostingVoice.MidiProgram,
                    groundHostingVoice.Status,
                    IsUserApplied: false
                )
            );

            // act
            component.ClickButtonByText("Randomize");

            // assert
            var state = _testContext.StateOf<CompositionConfigurationState>();

            if (state.GroundBassPattern is { } pattern)
            {
                groundBassFeasibilityAnalyzer.GetFeasibleGroundBasses(
                    _testContext.StateOf<InstrumentConfigurationState>().EnabledConfigurations,
                    state.Scale
                ).Should().Contain(pattern, "the rolled pattern must fit the ranges the rolled key {0} {1} snaps the voices to", state.TonicNote, state.Mode);
            }
        }
    }

    [Test]
    public void Reset_restores_the_default_composition_configuration()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationPanel>();

        component.FindAll("input[type=number]")[1].Change("90");

        // act
        component.ClickButtonByText("Reset");

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Tempo.Should().Be(120);
    }
}

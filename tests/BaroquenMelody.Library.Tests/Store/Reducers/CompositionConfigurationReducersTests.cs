using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class CompositionConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateCompositionConfiguration_updates_composition_configurations_as_expected()
    {
        // arrange
        var scale = BaroquenScale.Parse("G Minor");
        var state = new CompositionConfigurationState();

        // act
        state = CompositionConfigurationReducers.ReduceUpdateCompositionConfiguration(state, new UpdateCompositionConfiguration(scale, Meter.ThreeFour, 8));

        // assert
        state.Meter.Should().Be(Meter.ThreeFour);
        state.CompositionLength.Should().Be(8);

        state.Scale.Tonic.Should().Be(scale.Tonic);
        state.Scale.Supertonic.Should().Be(scale.Supertonic);
        state.Scale.Mediant.Should().Be(scale.Mediant);
        state.Scale.Subdominant.Should().Be(scale.Subdominant);
        state.Scale.Dominant.Should().Be(scale.Dominant);
        state.Scale.Submediant.Should().Be(scale.Submediant);
        state.Scale.LeadingTone.Should().Be(scale.LeadingTone);
        state.Scale.GetNotes().Should().BeEquivalentTo(scale.GetNotes());
    }
}

using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class SavedCompositionConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateLastLoadedConfigurationName()
    {
        // arrange
        var state = new SavedCompositionConfigurationState("LastLoadedConfigurationName");
        var action = new UpdateLastLoadedConfigurationName("NewLastLoadedConfigurationName");

        // act
        var newState = SavedCompositionConfigurationReducers.ReduceUpdateLastLoadedConfigurationName(state, action);

        // assert
        newState.LastLoadedConfigurationName.Should().Be("NewLastLoadedConfigurationName");
    }
}

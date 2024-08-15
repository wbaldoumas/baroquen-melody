using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class CompositionProgressReducersTests
{
    [Test]
    public void ReduceUpdateCompositionProgressAction_returns_new_state_with_expected_steps()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), CompositionStep.Waiting);
        var action = new ProgressCompositionStepAction(CompositionStep.Theme);

        // act
        var newState = CompositionProgressReducers.ReduceUpdateCompositionProgressAction(state, action);

        // assert
        newState.CompletedSteps.Should().BeEquivalentTo(new HashSet<CompositionStep> { CompositionStep.Waiting });
        newState.CurrentStep.Should().Be(CompositionStep.Theme);
    }
}

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
        var action = new ProgressCompositionStep(CompositionStep.Theme);

        // act
        var newState = CompositionProgressReducers.ReduceUpdateCompositionProgressAction(state, action);

        // assert
        newState.CompletedSteps.Should().BeEquivalentTo(new HashSet<CompositionStep> { CompositionStep.Waiting });
        newState.CurrentStep.Should().Be(CompositionStep.Theme);
        newState.Message.Should().Be("Composing main theme...");
        newState.ThemeProgress.Should().Be(0.0d);
        newState.BodyProgress.Should().Be(0.0d);
        newState.EndingProgress.Should().Be(0.0d);
        newState.OverallProgress.Should().Be(0.0d);
    }

    [Test]
    public void ReduceUpdateCompositionProgressAction_returns_new_state_with_expected_progress()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), CompositionStep.Theme);

        var progressThemeAction = new ProgressCompositionThemeProgress(0.25);
        var progressBodyAction = new ProgressCompositionBodyProgress(0.25);
        var progressEndingAction = new ProgressCompositionEndingProgress(0.25);

        // act
        var newState = CompositionProgressReducers.ReduceProgressCompositionThemeProgressAction(state, progressThemeAction);

        newState = CompositionProgressReducers.ReduceProgressCompositionBodyProgressAction(newState, progressBodyAction);
        newState = CompositionProgressReducers.ReduceProgressCompositionEndingProgressAction(newState, progressEndingAction);

        // assert
        newState.CompletedSteps.Should().BeEmpty();
        newState.CurrentStep.Should().Be(CompositionStep.Theme);
        newState.Message.Should().Be("Composing main theme...");
        newState.ThemeProgress.Should().Be(0.25d);
        newState.BodyProgress.Should().Be(0.25d);
        newState.EndingProgress.Should().Be(0.25d);
        newState.OverallProgress.Should().Be(0.25d);
    }

    [Test]
    public void ReduceResetCompositionProgressAction_returns_new_state_with_expected_values()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep> { CompositionStep.Theme }, CompositionStep.Theme);

        // act
        var newState = CompositionProgressReducers.ReduceResetCompositionProgressAction(state, new ResetCompositionProgress());

        // assert
        newState.CompletedSteps.Should().BeEmpty();
        newState.CurrentStep.Should().Be(CompositionStep.Waiting);
        newState.Message.Should().Be("Waiting to compose...");
        newState.ThemeProgress.Should().Be(0.0d);
        newState.BodyProgress.Should().Be(0.0d);
        newState.EndingProgress.Should().Be(0.0d);
        newState.OverallProgress.Should().Be(0.0d);
    }

    [Test]
    public void ReduceMarkCompositionFailed_returns_new_state_with_expected_values()
    {
        // arrange
        var state = new CompositionProgressState(
            new HashSet<CompositionStep> { CompositionStep.Theme },
            CompositionStep.Theme,
            ThemeProgress: 0.25d,
            BodyProgress: 0.25d,
            EndingProgress: 0.25d
        );

        var action = new MarkCompositionFailed();

        // act
        var newState = CompositionProgressReducers.ReduceMarkCompositionFailed(state, action);

        // assert
        newState.CompletedSteps.Should().BeEmpty();
        newState.CurrentStep.Should().Be(CompositionStep.Failed);
        newState.Message.Should().Be("Failed to compose composition.");
        newState.ThemeProgress.Should().Be(0.0d);
        newState.BodyProgress.Should().Be(0.0d);
        newState.EndingProgress.Should().Be(0.0d);
        newState.OverallProgress.Should().Be(0.0d);
    }
}

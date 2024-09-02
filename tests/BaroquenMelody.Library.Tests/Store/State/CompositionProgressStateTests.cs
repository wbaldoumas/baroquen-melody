using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.State;

[TestFixture]
internal sealed class CompositionProgressStateTests
{
    [Test]
    public void OverallProgress_returns_average_of_theme_body_and_ending_progress()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), CompositionStep.Theme, 0.25d, 0.25d, 0.25d);

        // act
        state.OverallProgress.Should().Be(0.25d);
    }

    [Test]
    [TestCase(CompositionStep.Waiting, "Waiting to compose...")]
    [TestCase(CompositionStep.Theme, "Composing main theme...")]
    [TestCase(CompositionStep.Body, "Continuing composition...")]
    [TestCase(CompositionStep.Ornamentation, "Composing ending...")]
    [TestCase(CompositionStep.Phrasing, "Composing ending...")]
    [TestCase(CompositionStep.Ending, "Composing ending...")]
    [TestCase(CompositionStep.Complete, "Composition complete!")]
    public void Message_returns_expected_message_for_current_step(CompositionStep step, string expectedMessage)
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), step);

        // act
        state.Message.Should().Be(expectedMessage);
    }

    [Test]
    public void Message_throws_when_current_step_is_unknown()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), (CompositionStep)byte.MaxValue);

        // act
        var act = () => _ = state.Message;

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void IsComplete_returns_true_when_current_step_is_complete()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), CompositionStep.Complete);

        // act
        state.IsComplete.Should().BeTrue();
    }

    [Test]
    public void IsWaiting_returns_true_when_current_step_is_waiting()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), CompositionStep.Waiting);

        // act
        state.IsWaiting.Should().BeTrue();
    }

    [Test]
    public void IsLoading_returns_true_when_current_step_is_not_complete_and_not_waiting()
    {
        // arrange
        var state = new CompositionProgressState(new HashSet<CompositionStep>(), CompositionStep.Theme);

        // act
        state.IsLoading.Should().BeTrue();
    }
}

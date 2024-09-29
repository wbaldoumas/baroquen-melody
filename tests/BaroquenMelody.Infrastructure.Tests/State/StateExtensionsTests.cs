using BaroquenMelody.Infrastructure.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.State;

[TestFixture]
internal sealed class StateExtensionsTests
{
    private TestState _testState = null!;

    [SetUp]
    public void SetUp() => _testState = new TestState();

    [Test]
    public void ObserveChanges_returns_IObservable_of_state_changes_which_can_be_subscribed_to()
    {
        var stateChanges = new List<int> { 1, 2, 3, 4, 5 };
        var expectedStateChanges = new List<int>();

        // arrange
        using var stateSubscription = _testState
            .ObserveChanges()
            .Subscribe(state => expectedStateChanges.Add(state.Value));

        // act
        stateChanges.ForEach(stateChange => _testState.SetValue(stateChange));

        // assert
        expectedStateChanges.Should().BeEquivalentTo(stateChanges);
    }
}

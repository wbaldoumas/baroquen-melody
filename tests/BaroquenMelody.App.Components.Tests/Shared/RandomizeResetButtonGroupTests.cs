using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class RandomizeResetButtonGroupTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Clicking_the_randomize_button_invokes_the_randomize_callback()
    {
        // arrange
        var wasRandomizeClicked = false;
        var wasResetClicked = false;
        var component = RenderButtonGroup(() => wasRandomizeClicked = true, () => wasResetClicked = true);

        // act
        component.FindAll("button")[0].Click();

        // assert
        wasRandomizeClicked.Should().BeTrue();
        wasResetClicked.Should().BeFalse();
    }

    [Test]
    public void Clicking_the_reset_button_invokes_the_reset_callback()
    {
        // arrange
        var wasRandomizeClicked = false;
        var wasResetClicked = false;
        var component = RenderButtonGroup(() => wasRandomizeClicked = true, () => wasResetClicked = true);

        // act
        component.FindAll("button")[1].Click();

        // assert
        wasResetClicked.Should().BeTrue();
        wasRandomizeClicked.Should().BeFalse();
    }

    [Test]
    public void Buttons_are_labeled_randomize_and_reset()
    {
        // act
        var component = RenderButtonGroup(() => { }, () => { });

        // assert
        var buttons = component.FindAll("button");

        buttons[0].TextContent.Should().Contain("Randomize");
        buttons[1].TextContent.Should().Contain("Reset");
    }

    private IRenderedComponent<RandomizeResetButtonGroup> RenderButtonGroup(Action onRandomize, Action onReset) => _testContext.RenderComponent<RandomizeResetButtonGroup>(parameters => parameters
        .Add(component => component.OnRandomizeClick, onRandomize)
        .Add(component => component.OnResetClick, onReset)
    );
}

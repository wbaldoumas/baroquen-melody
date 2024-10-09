using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration.Services;

[TestFixture]
internal sealed class CompositionConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private CompositionConfigurationService _compositionConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();

        _compositionConfigurationService = new CompositionConfigurationService(_mockDispatcher);
    }

    [Test]
    public void ConfigurableRootNotes_returns_expected_values()
    {
        // arrange
        var expectedConfigurableRootNotes = new[]
        {
            NoteName.C,
            NoteName.CSharp,
            NoteName.D,
            NoteName.DSharp,
            NoteName.E,
            NoteName.F,
            NoteName.FSharp,
            NoteName.G,
            NoteName.GSharp,
            NoteName.A,
            NoteName.ASharp,
            NoteName.B
        };

        // act
        var actualConfigurableRootNotes = _compositionConfigurationService.ConfigurableRootNotes;

        // assert
        actualConfigurableRootNotes.Should().BeEquivalentTo(expectedConfigurableRootNotes);
    }

    [Test]
    public void ConfigurableScaleModes_returns_expected_values()
    {
        // arrange
        var expectedConfigurableScaleModes = new[]
        {
            Mode.Ionian,
            Mode.Dorian,
            Mode.Phrygian,
            Mode.Lydian,
            Mode.Mixolydian,
            Mode.Aeolian,
            Mode.Locrian
        };

        // act
        var actualConfigurableScaleModes = _compositionConfigurationService.ConfigurableScaleModes;

        // assert
        actualConfigurableScaleModes.Should().BeEquivalentTo(expectedConfigurableScaleModes);
    }

    [Test]
    public void ConfigurableMeters_returns_expected_values()
    {
        // arrange
        var expectedConfigurableMeters = new[]
        {
            Meter.FourFour,
            Meter.ThreeFour,
            Meter.FiveEight
        };

        // act
        var actualConfigurableMeters = _compositionConfigurationService.ConfigurableMeters;

        // assert
        actualConfigurableMeters.Should().BeEquivalentTo(expectedConfigurableMeters);
    }

    [Test]
    public void Randomize_dispatches_expected_update()
    {
        // act
        _compositionConfigurationService.Randomize();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Any<UpdateCompositionConfiguration>());
    }

    [Test]
    public void Reset_dispatches_expected_update()
    {
        // act
        _compositionConfigurationService.Reset();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Any<UpdateCompositionConfiguration>());
    }
}
